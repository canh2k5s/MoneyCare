using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers
{
    // Issue 4: thêm, sửa, xóa, lịch sử và lọc giao dịch theo đúng tài khoản.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class GiaoDichController : Controller
    {
        private const int KichThuocTrang = 20;
        private readonly MoneyCareContext _context;

        public GiaoDichController(MoneyCareContext context)
        {
            _context = context;
        }

        private async Task<int?> NguoiDungHienTai()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue &&
                await _context.NguoiDung.AsNoTracking()
                    .AnyAsync(u => u.UserId == userId.Value))
                return userId.Value;

            HttpContext.Session.Clear();
            return null;
        }

        // GET /GiaoDich?TuKhoa=...&Loai=Thu&Thang=11&Nam=2026
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] GiaoDichBoLoc boLoc, int trang = 1)
        {
            int? userId = await NguoiDungHienTai();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = new GiaoDichPageViewModel
            {
                BoLoc = boLoc,
                Form = new ThemGiaoDichInput { Loai = "Chi", NgayGiaoDich = DateTime.Today }
            };
            if (boLoc.TuNgay?.Date > boLoc.DenNgay?.Date)
                ModelState.AddModelError(nameof(boLoc.DenNgay), "Ngày kết thúc phải từ ngày bắt đầu trở đi.");
            await NapDanhMuc(model, userId.Value);
            await NapLichSu(model, userId.Value, trang);
            return View(model);
        }

        // POST /GiaoDich/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them([Bind(Prefix = "Form")] ThemGiaoDichInput form)
        {
            int? userId = await NguoiDungHienTai();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            decimal soTien = await KiemTraForm(form, userId.Value);
            if (!ModelState.IsValid)
            {
                var model = new GiaoDichPageViewModel { Form = form };
                await NapDanhMuc(model, userId.Value);
                await NapLichSu(model, userId.Value, 1);
                return View("Index", model);
            }

            var giaoDich = new GiaoDich
            {
                UserId = userId.Value, // Không bao giờ lấy UserId từ form.
                MaDanhMuc = form.MaDanhMuc,
                SoTien = soTien,
                NgayGiaoDich = form.NgayGiaoDich!.Value.Date,
                GhiChu = ChuanHoaGhiChu(form.GhiChu)
            };

            _context.GiaoDich.Add(giaoDich);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã thêm giao dịch thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET /GiaoDich/Sua/123 - chỉ mở giao dịch thuộc UserId hiện tại.
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            int? userId = await NguoiDungHienTai();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var giaoDich = await _context.GiaoDich.AsNoTracking()
                .Include(g => g.DanhMuc)
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.UserId == userId.Value);
            if (giaoDich == null) return NotFound();

            var model = new SuaGiaoDichViewModel
            {
                MaGiaoDich = id,
                Form = new ThemGiaoDichInput
                {
                    Loai = giaoDich.DanhMuc?.Loai ?? "Chi",
                    MaDanhMuc = giaoDich.MaDanhMuc,
                    SoTien = giaoDich.SoTien.ToString("0.##", CultureInfo.InvariantCulture),
                    NgayGiaoDich = giaoDich.NgayGiaoDich.Date,
                    GhiChu = giaoDich.GhiChu
                },
                DanhMuc = await LayDanhMuc(userId.Value)
            };
            return View(model);
        }

        // POST /GiaoDich/Sua/123 - xác minh lại quyền sở hữu trước khi ghi.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, [Bind(Prefix = "Form")] ThemGiaoDichInput form)
        {
            int? userId = await NguoiDungHienTai();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var giaoDich = await _context.GiaoDich
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.UserId == userId.Value);
            if (giaoDich == null) return NotFound();

            decimal soTien = await KiemTraForm(form, userId.Value);
            if (!ModelState.IsValid)
            {
                return View(new SuaGiaoDichViewModel
                {
                    MaGiaoDich = id,
                    Form = form,
                    DanhMuc = await LayDanhMuc(userId.Value)
                });
            }

            giaoDich.MaDanhMuc = form.MaDanhMuc;
            giaoDich.SoTien = soTien;
            giaoDich.NgayGiaoDich = form.NgayGiaoDich!.Value.Date;
            giaoDich.GhiChu = ChuanHoaGhiChu(form.GhiChu);
            // UserId giữ nguyên, không cho chuyển giao dịch sang tài khoản khác.
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã cập nhật giao dịch thành công.";
            return RedirectToAction(nameof(Index));
        }

        // Xóa bằng POST kèm anti-forgery, KHÔNG xóa qua link GET.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            int? userId = await NguoiDungHienTai();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var giaoDich = await _context.GiaoDich
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.UserId == userId.Value);
            if (giaoDich == null) return NotFound();

            _context.GiaoDich.Remove(giaoDich);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã xóa giao dịch.";
            return RedirectToAction(nameof(Index));
        }

        private static string? ChuanHoaGhiChu(string? ghiChu) =>
            string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();

        private async Task<decimal> KiemTraForm(ThemGiaoDichInput input, int userId)
        {
            // Form HTML gửi dấu chấm thập phân bất kể ngôn ngữ của máy chủ.
            if (!decimal.TryParse(input.SoTien, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal soTien)
                || soTien <= 0 || soTien > 9999999999999999.99m || decimal.Round(soTien, 2) != soTien)
                ModelState.AddModelError("Form.SoTien", "Nhập số tiền dương với tối đa 2 chữ số thập phân (ví dụ 125000.50).");

            if (input.Loai is not ("Thu" or "Chi"))
                ModelState.AddModelError("Form.Loai", "Loại giao dịch chỉ được là Thu hoặc Chi.");

            // Không chấp nhận danh mục thuộc tài khoản khác hoặc sai loại Thu/Chi.
            bool hopLe = await _context.DanhMuc.AsNoTracking()
                .AnyAsync(d => d.Id == input.MaDanhMuc && d.UserId == userId && d.Loai == input.Loai);
            if (!hopLe)
                ModelState.AddModelError("Form.MaDanhMuc", "Danh mục không hợp lệ hoặc không thuộc tài khoản.");

            return soTien;
        }

        private Task<List<DanhMuc>> LayDanhMuc(int userId) =>
            _context.DanhMuc.AsNoTracking()
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Loai)
                .ThenBy(d => d.Ten)
                .ToListAsync();

        private async Task NapDanhMuc(GiaoDichPageViewModel model, int userId)
        {
            model.DanhMuc = await LayDanhMuc(userId);
        }

        private async Task NapLichSu(GiaoDichPageViewModel model, int userId, int trang)
        {
            IQueryable<GiaoDich> q = _context.GiaoDich.AsNoTracking()
                .Where(g => g.UserId == userId); // Bảo vệ dữ liệu người dùng ở DB.

            var f = model.BoLoc;
            if (!string.IsNullOrWhiteSpace(f.TuKhoa))
            {
                string tuKhoa = f.TuKhoa.Trim();
                if (tuKhoa.Length > 200) tuKhoa = tuKhoa[..200];
                f.TuKhoa = tuKhoa;
                q = q.Where(g => g.GhiChu != null && g.GhiChu.Contains(tuKhoa));
            }

            if (f.Loai is "Thu" or "Chi")
                q = q.Where(g => g.DanhMuc != null && g.DanhMuc.Loai == f.Loai);
            else
                f.Loai = null;

            if (f.MaDanhMuc.HasValue && f.MaDanhMuc > 0)
                q = q.Where(g => g.MaDanhMuc == f.MaDanhMuc.Value);
            else
                f.MaDanhMuc = null;

            if (f.TuNgay.HasValue)
                q = q.Where(g => g.NgayGiaoDich >= f.TuNgay.Value.Date);
            if (f.DenNgay.HasValue)
            {
                // Inclusive end date, không dùng AddDays trên DateTime.MaxValue.
                var den = f.DenNgay.Value.Date;
                if (den < DateTime.MaxValue.Date)
                {
                    DateTime ngaySau = den.AddDays(1);
                    q = q.Where(g => g.NgayGiaoDich < ngaySau);
                }
                else
                    q = q.Where(g => g.NgayGiaoDich <= DateTime.MaxValue);
            }

            if (f.Thang is >= 1 and <= 12)
                q = q.Where(g => g.NgayGiaoDich.Month == f.Thang.Value);
            else
                f.Thang = null;

            if (f.Nam is >= 1 and <= 9999)
                q = q.Where(g => g.NgayGiaoDich.Year == f.Nam.Value);
            else
                f.Nam = null;

            model.TongBanGhi = await q.CountAsync();
            model.TongThu = await q.Where(g => g.DanhMuc != null && g.DanhMuc.Loai == "Thu")
                .SumAsync(g => (decimal?)g.SoTien) ?? 0m;
            model.TongChi = await q.Where(g => g.DanhMuc != null && g.DanhMuc.Loai == "Chi")
                .SumAsync(g => (decimal?)g.SoTien) ?? 0m;

            model.SoTrang = Math.Max(1, (int)(((long)model.TongBanGhi + KichThuocTrang - 1) / KichThuocTrang));
            model.Trang = Math.Clamp(trang, 1, model.SoTrang);

            model.DanhSach = await q.Include(g => g.DanhMuc)
                .OrderByDescending(g => g.NgayGiaoDich)
                .ThenByDescending(g => g.MaGiaoDich)
                .Skip((model.Trang - 1) * KichThuocTrang)
                .Take(KichThuocTrang)
                .ToListAsync();
        }
    }
}
