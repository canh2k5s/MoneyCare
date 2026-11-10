
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;
using System.Security.Claims;

namespace MoneyCare.Controllers
{
    [Authorize]
    public class NganSachController : Controller
    {
        private readonly MoneyCareContext _context;

        public NganSachController(MoneyCareContext context)
        {
            _context = context;
        }

        // Lấy UserId từ tài khoản đăng nhập
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        // Chuyển UserId sang int để dùng với DanhMuc và GiaoDich
        private bool TryGetUserIdInt(out int userId)
        {
            return int.TryParse(GetUserId(), out userId);
        }

        // 1. Danh sách ngân sách
        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            string userIdString = userId.ToString();

            int currentMonth = thang ?? DateTime.Now.Month;
            int currentYear = nam ?? DateTime.Now.Year;

            if (currentMonth < 1 || currentMonth > 12)
                return BadRequest();

            var nganSaches = await _context.NganSach
                .Include(n => n.DanhMuc)
                .Where(n =>
                    n.UserId == userIdString &&
                    n.Thang == currentMonth &&
                    n.Nam == currentYear)
                .AsNoTracking()
                .ToListAsync();

            var listViewModel = new List<NganSachViewModel>();

            foreach (var ns in nganSaches)
            {
                decimal tongChi = await _context.GiaoDich
                    .Where(g =>
                        g.UserId == userId &&
                        g.MaDanhMuc == ns.MaDanhMuc &&
                        g.NgayGiaoDich.Month == ns.Thang &&
                        g.NgayGiaoDich.Year == ns.Nam &&
                        g.DanhMuc != null &&
                        g.DanhMuc.Loai == "Chi")
                    .SumAsync(g => (decimal?)g.SoTien) ?? 0;

                decimal conLai = ns.HanMuc - tongChi;

                double phanTram = ns.HanMuc > 0
                    ? (double)(tongChi / ns.HanMuc) * 100
                    : 0;

                listViewModel.Add(new NganSachViewModel
                {
                    NganSach = ns,
                    TongChi = tongChi,
                    ConLai = conLai,
                    PhanTramSuDung = phanTram
                });
            }

            ViewBag.Thang = currentMonth;
            ViewBag.Nam = currentYear;

            return View(listViewModel);
        }

        // 2. GET Create
        public async Task<IActionResult> Create()
        {
            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            await LoadDanhMucAsync(userId);

            return View();
        }

        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NganSach nganSach)
        {
            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            string userIdString = userId.ToString();

            // Gán tài khoản hiện tại
            nganSach.UserId = userIdString;

            // Kiểm tra danh mục có thuộc tài khoản và là danh mục chi không
            bool danhMucHopLe = await _context.DanhMuc
                .AnyAsync(d =>
                    d.Id == nganSach.MaDanhMuc &&
                    d.UserId == userId &&
                    d.Loai == "Chi");

            if (!danhMucHopLe)
            {
                ModelState.AddModelError(
                    nameof(NganSach.MaDanhMuc),
                    "Danh mục không hợp lệ.");
            }

            // Kiểm tra ngân sách đã tồn tại
            bool existed = await _context.NganSach
                .AnyAsync(n =>
                    n.UserId == userIdString &&
                    n.MaDanhMuc == nganSach.MaDanhMuc &&
                    n.Thang == nganSach.Thang &&
                    n.Nam == nganSach.Nam);

            if (existed)
            {
                ModelState.AddModelError(
                    "",
                    "Ngân sách cho danh mục này trong tháng đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.NganSach.Add(nganSach);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDanhMucAsync(userId, nganSach.MaDanhMuc);

            return View(nganSach);
        }

        // 3. GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            string userIdString = userId.ToString();

            var nganSach = await _context.NganSach
                .FirstOrDefaultAsync(n =>
                    n.MaNganSach == id &&
                    n.UserId == userIdString);

            if (nganSach == null)
                return NotFound();

            await LoadDanhMucAsync(userId, nganSach.MaDanhMuc);

            return View(nganSach);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NganSach nganSach)
        {
            if (id != nganSach.MaNganSach)
                return NotFound();

            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            string userIdString = userId.ToString();

            var entity = await _context.NganSach
                .FirstOrDefaultAsync(n =>
                    n.MaNganSach == id &&
                    n.UserId == userIdString);

            if (entity == null)
                return NotFound();

            // Không cho thay đổi chủ sở hữu ngân sách
            nganSach.UserId = userIdString;

            bool danhMucHopLe = await _context.DanhMuc
                .AnyAsync(d =>
                    d.Id == nganSach.MaDanhMuc &&
                    d.UserId == userId &&
                    d.Loai == "Chi");

            if (!danhMucHopLe)
            {
                ModelState.AddModelError(
                    nameof(NganSach.MaDanhMuc),
                    "Danh mục không hợp lệ.");
            }

            bool trung = await _context.NganSach
                .AnyAsync(n =>
                    n.UserId == userIdString &&
                    n.MaDanhMuc == nganSach.MaDanhMuc &&
                    n.Thang == nganSach.Thang &&
                    n.Nam == nganSach.Nam &&
                    n.MaNganSach != id);

            if (trung)
            {
                ModelState.AddModelError(
                    "",
                    "Đã tồn tại ngân sách cho danh mục này trong tháng.");
            }

            if (ModelState.IsValid)
            {
                entity.MaDanhMuc = nganSach.MaDanhMuc;
                entity.Thang = nganSach.Thang;
                entity.Nam = nganSach.Nam;
                entity.HanMuc = nganSach.HanMuc;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDanhMucAsync(userId, nganSach.MaDanhMuc);

            return View(nganSach);
        }

        // 4. Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserIdInt(out int userId))
                return Unauthorized();

            string userIdString = userId.ToString();

            var nganSach = await _context.NganSach
                .FirstOrDefaultAsync(n =>
                    n.MaNganSach == id &&
                    n.UserId == userIdString);

            if (nganSach != null)
            {
                _context.NganSach.Remove(nganSach);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. Nạp danh mục chi tiêu
        private async Task LoadDanhMucAsync(
            int userId,
            int? selected = null)
        {
            var list = await _context.DanhMuc
                .Where(d =>
                    d.UserId == userId &&
                    d.Loai == "Chi")
                .ToListAsync();

            ViewBag.DanhMucs = new SelectList(
                list,
                "Id",
                "Ten",
                selected);
        }
    }

    // ViewModel hiển thị ngân sách
    public class NganSachViewModel
    {
        public NganSach NganSach { get; set; } = default!;

        public decimal TongChi { get; set; }

        public decimal ConLai { get; set; }

        public double PhanTramSuDung { get; set; }
    }
}
