using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers
{
    // Chuc nang quan ly giao dich duoc tach rieng,
    // khong can thay the HomeController cua du an goc.
    public class GiaoDichController : Controller
    {
        private readonly MoneyCareContext _context;

        public GiaoDichController(MoneyCareContext context)
        {
            _context = context;
        }

        // GET: /GiaoDich
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return RedirectToAction("Login", "Account");

            if (!await _context.NguoiDung.AnyAsync(u => u.UserId == userId.Value))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var model = new GiaoDichPageViewModel();
            await NapDuLieuTrang(model, userId.Value);
            return View(model);
        }

        // POST: /GiaoDich/Them - luu giao dich vao SQL Server
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(GiaoDichPageViewModel model)
        {
            // UserId phai lay tu Session, khong nhan tu form.
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return RedirectToAction("Login", "Account");

            if (!await _context.NguoiDung.AnyAsync(u => u.UserId == userId.Value))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            if (model.Form is null)
            {
                model.Form = new ThemGiaoDichInput();
                ModelState.AddModelError(string.Empty, "Thong tin giao dich khong hop le.");
            }

            // SQL Server: decimal(18,2) nen khong nhan hon 2 chu so thap phan.
            if (decimal.Round(model.Form.SoTien, 2) != model.Form.SoTien)
            {
                ModelState.AddModelError("Form.SoTien", "Số tiền chỉ được có tối đa 2 chữ số thập phân.");
            }

            // Kiem tra danh muc thuoc dung tai khoan va dung loai Thu/Chi.
            bool danhMucHopLe = await _context.DanhMuc.AnyAsync(d =>
                d.MaDanhMuc == model.Form.MaDanhMuc &&
                d.UserId == userId.Value &&
                d.Loai == model.Form.Loai);

            if (!danhMucHopLe)
            {
                ModelState.AddModelError("Form.MaDanhMuc", "Danh mục không hợp lệ hoặc không thuộc tài khoản.");
            }

            if (!ModelState.IsValid)
            {
                await NapDuLieuTrang(model, userId.Value);
                return View("Index", model);
            }

            var giaoDich = new GiaoDich
            {
                UserId = userId.Value,
                MaDanhMuc = model.Form.MaDanhMuc,
                SoTien = model.Form.SoTien,
                NgayGiaoDich = model.Form.NgayGiaoDich!.Value.Date,
                GhiChu = model.Form.GhiChu?.Trim()
            };

            _context.GiaoDich.Add(giaoDich);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Lưu giao dịch thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Danh muc va lich su chi duoc loc theo UserId dang dang nhap.
        private async Task NapDuLieuTrang(GiaoDichPageViewModel model, int userId)
        {
            model.DanhMuc = await _context.DanhMuc
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Loai)
                .ThenBy(d => d.TenDanhMuc)
                .ToListAsync();

            model.DanhSach = await _context.GiaoDich
                .AsNoTracking()
                .Include(g => g.DanhMuc)
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.NgayGiaoDich)
                .ThenByDescending(g => g.MaGiaoDich)
                .ToListAsync();
        }
    }
}
