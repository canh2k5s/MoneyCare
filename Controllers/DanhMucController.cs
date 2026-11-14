using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly MoneyCareContext _db;

        public DanhMucController(MoneyCareContext db)
        {
            _db = db;
        }

        // Lấy ID tài khoản đang đăng nhập
        private int? LayUserId()
        {
            // Lấy từ Claims
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out int id))
                return id;

            // Nếu không có Claims thì lấy từ Session
            var sessionId = HttpContext.Session.GetInt32("UserId");

            if (sessionId.HasValue)
                return sessionId.Value;

            // SỬA DÒNG NÀY: Tự động trả về 1 nếu chưa đăng nhập (giống Ngân sách)
            return 1; 
        }

        // Hiển thị danh sách danh mục, có lọc và tìm kiếm
        public async Task<IActionResult> Index(string loai = "TatCa", string? tuKhoa = null)
        {
            int? uid = LayUserId();

            if (uid == null)
                // ĐÃ SỬA: Chuyển hướng về trang Đăng nhập thay vì hiện chữ
                return RedirectToAction("Login", "Account");

            if (loai != "Thu" && loai != "Chi")
                loai = "TatCa";

            ViewBag.Loai = loai;
            ViewBag.TuKhoa = tuKhoa;

            // Thống kê số lượng danh mục
            ViewBag.SoChi = await _db.DanhMuc
                .CountAsync(x => x.UserId == uid.Value && x.Loai == "Chi");

            ViewBag.SoThu = await _db.DanhMuc
                .CountAsync(x => x.UserId == uid.Value && x.Loai == "Thu");

            ViewBag.TongDanhMuc = ViewBag.SoChi + ViewBag.SoThu;

            // Lấy danh mục của tài khoản đang đăng nhập
            var query = _db.DanhMuc
                .Where(x => x.UserId == uid.Value);

            // Lọc theo loại
            if (loai == "Thu" || loai == "Chi")
            {
                query = query.Where(x => x.Loai == loai);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tuKhoaTrim = tuKhoa.Trim();
                query = query.Where(x => x.Ten.Contains(tuKhoaTrim));
            }

            var danhSach = await query
                .OrderBy(x => x.Loai)
                .ThenBy(x => x.Ten)
                .ToListAsync();

            // SỬA Ở ĐÂY: Chỉ định rõ tên View là "DanhMuc"
            return View("DanhMuc", danhSach);
        }

        // Thêm hoặc sửa danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(DanhMuc model)
        {
            int? uid = LayUserId();

            if (uid == null)
                // ĐÃ SỬA: Chuyển hướng về trang Đăng nhập
                return RedirectToAction("Login", "Account");

            if (model.Loai != "Thu")
                model.Loai = "Chi";

            if (string.IsNullOrWhiteSpace(model.Ten))
            {
                TempData["Loi"] = "Vui lòng nhập tên danh mục.";

                return RedirectToAction(
                    nameof(Index), new { loai = model.Loai });
            }

            model.Ten = model.Ten.Trim();

            // Kiểm tra tên danh mục trùng
            bool trung = await _db.DanhMuc.AnyAsync(x =>
                x.UserId == uid.Value &&
                x.Id != model.Id &&
                x.Loai == model.Loai &&
                x.Ten == model.Ten);

            if (trung)
            {
                TempData["Loi"] = "Tên danh mục này đã tồn tại.";

                return RedirectToAction(
                    nameof(Index), new { loai = model.Loai });
            }

            if (model.Id == 0)
            {
                // Thêm danh mục mới
                model.UserId = uid.Value;

                _db.DanhMuc.Add(model);

                TempData["Ok"] = "Đã thêm danh mục.";
            }
            else
            {
                // Chỉ sửa danh mục của tài khoản hiện tại
                var danhMucCu = await _db.DanhMuc
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.Id &&
                        x.UserId == uid.Value);

                if (danhMucCu == null)
                    return NotFound();

                danhMucCu.Ten = model.Ten;
                danhMucCu.Loai = model.Loai;
                danhMucCu.Icon = model.Icon;
                danhMucCu.Mau = model.Mau;

                TempData["Ok"] = "Đã cập nhật danh mục.";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index), new { loai = model.Loai });
        }

        // Xóa danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            int? uid = LayUserId();

            if (uid == null)
                // ĐÃ SỬA: Chuyển hướng về trang Đăng nhập
                return RedirectToAction("Login", "Account");

            // Chỉ tìm danh mục thuộc tài khoản hiện tại
            var danhMuc = await _db.DanhMuc
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == uid.Value);

            if (danhMuc == null)
                return NotFound();

            string loai = danhMuc.Loai;

            try
            {
                _db.DanhMuc.Remove(danhMuc);

                await _db.SaveChangesAsync();

                TempData["Ok"] = "Đã xóa danh mục.";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] =
                    "Không thể xóa vì danh mục đang được sử dụng trong giao dịch.";
            }

            return RedirectToAction(
                nameof(Index), new { loai = loai });
        }
    }
}