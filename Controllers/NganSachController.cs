using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers
{
    public class NganSachController : Controller
    {
        private readonly MoneyCareContext _context;

        public NganSachController(MoneyCareContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // GET: /NganSach hoặc /NganSach/Index
        public async Task<IActionResult> Index(byte? thang, short? nam)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            DateTime today = DateTime.Today;
            byte selectedMonth = thang ?? (byte)today.Month;
            short selectedYear = nam ?? (short)today.Year;

            if (selectedMonth < 1 || selectedMonth > 12)
                selectedMonth = (byte)today.Month;

            ViewBag.Thang = selectedMonth;
            ViewBag.Nam = selectedYear;

            DateTime fromDate = new DateTime(selectedYear, selectedMonth, 1);
            DateTime toDate = fromDate.AddMonths(1);

            // Lấy ngân sách và tên danh mục thuộc đúng tài khoản.
            var budgets = await (
                from n in _context.NganSach.AsNoTracking()
                join d in _context.DanhMuc.AsNoTracking()
                    on n.MaDanhMuc equals d.Id
                where n.UserId == userId.Value
                      && d.UserId == userId.Value
                      && d.Loai == "Chi"
                      && n.Thang == selectedMonth
                      && n.Nam == selectedYear
                orderby d.Ten
                select new
                {
                    n.MaNganSach,
                    n.UserId,
                    n.Thang,
                    n.Nam,
                    n.MaDanhMuc,
                    TenDanhMuc = d.Ten,
                    n.HanMuc
                }).ToListAsync();

            // Chỉ cộng giao dịch thuộc danh mục chi tiêu của chính người dùng.
            var expenseGroups = await (
                from g in _context.GiaoDich.AsNoTracking()
                join d in _context.DanhMuc.AsNoTracking()
                    on g.MaDanhMuc equals d.Id
                where g.UserId == userId.Value
                      && d.UserId == userId.Value
                      && d.Loai == "Chi"
                      && g.NgayGiaoDich >= fromDate
                      && g.NgayGiaoDich < toDate
                group g by g.MaDanhMuc into grouped
                select new
                {
                    MaDanhMuc = grouped.Key,
                    DaChi = grouped.Sum(x => x.SoTien)
                }).ToListAsync();

            var expenseByCategory = expenseGroups.ToDictionary(
                x => x.MaDanhMuc,
                x => x.DaChi);

            var result = budgets.Select(n =>
            {
                decimal spent = expenseByCategory.TryGetValue(n.MaDanhMuc, out decimal value)
                    ? value : 0m;

                decimal remaining = n.HanMuc - spent;
                decimal percent = n.HanMuc > 0
                    ? Math.Round(spent / n.HanMuc * 100m, 2)
                    : 0m;

                string warning = spent > n.HanMuc
                    ? "VuotHanMuc"
                    : percent >= 80m
                        ? "SapVuot"
                        : "BinhThuong";

                return new NganSachTheoDoiViewModel
                {
                    MaNganSach = n.MaNganSach,
                    UserId = n.UserId,
                    Thang = n.Thang,
                    Nam = n.Nam,
                    MaDanhMuc = n.MaDanhMuc,
                    TenDanhMuc = n.TenDanhMuc,
                    HanMuc = n.HanMuc,
                    DaChi = spent,
                    ConLai = remaining,
                    PhanTramDaDung = percent,
                    CanhBao = warning
                };
            }).ToList();

            return View("~/Views/NganSach/NganSach.cshtml", result);
        }

        // GET: /NganSach/Create
        public async Task<IActionResult> Create()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            await LoadExpenseCategories(userId.Value);

            return View("~/Views/NganSach/Create.cshtml", new NganSach
            {
                Thang = (byte)DateTime.Today.Month,
                Nam = (short)DateTime.Today.Year
            });
        }

        // POST: /NganSach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaDanhMuc,Thang,Nam,HanMuc")] NganSach model)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            model.UserId = userId.Value;

            bool categoryValid = await _context.DanhMuc.AnyAsync(d =>
                d.Id == model.MaDanhMuc &&
                d.UserId == userId.Value &&
                d.Loai == "Chi");

            if (!categoryValid)
                ModelState.AddModelError(nameof(model.MaDanhMuc), "Danh mục chi tiêu không hợp lệ.");

            bool duplicate = await _context.NganSach.AnyAsync(n =>
                n.UserId == userId.Value &&
                n.MaDanhMuc == model.MaDanhMuc &&
                n.Thang == model.Thang &&
                n.Nam == model.Nam);

            if (duplicate)
                ModelState.AddModelError("", "Danh mục này đã có ngân sách trong tháng/năm đã chọn.");

            if (ModelState.IsValid)
            {
                _context.NganSach.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thiết lập ngân sách thành công!";
                return RedirectToAction(nameof(Index), new { thang = model.Thang, nam = model.Nam });
            }

            await LoadExpenseCategories(userId.Value, model.MaDanhMuc);
            return View("~/Views/NganSach/Create.cshtml", model);
        }

        // GET: /NganSach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var model = await _context.NganSach
                .AsNoTracking()
                .FirstOrDefaultAsync(n =>
                    n.MaNganSach == id.Value && n.UserId == userId.Value);

            if (model == null)
                return NotFound();

            await LoadExpenseCategories(userId.Value, model.MaDanhMuc);
            return View("~/Views/NganSach/Edit.cshtml", model);
        }

        // POST: /NganSach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaNganSach,MaDanhMuc,Thang,Nam,HanMuc")] NganSach input)
        {
            if (id != input.MaNganSach)
                return NotFound();

            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Lấy bản ghi theo cả mã ngân sách và UserId để không sửa được
            // ngân sách thuộc tài khoản khác.
            var existing = await _context.NganSach.FirstOrDefaultAsync(n =>
                n.MaNganSach == id && n.UserId == userId.Value);

            if (existing == null)
                return NotFound();

            bool categoryValid = await _context.DanhMuc.AnyAsync(d =>
                d.Id == input.MaDanhMuc &&
                d.UserId == userId.Value &&
                d.Loai == "Chi");

            if (!categoryValid)
                ModelState.AddModelError(nameof(input.MaDanhMuc), "Danh mục chi tiêu không hợp lệ.");

            bool duplicate = await _context.NganSach.AnyAsync(n =>
                n.MaNganSach != id &&
                n.UserId == userId.Value &&
                n.MaDanhMuc == input.MaDanhMuc &&
                n.Thang == input.Thang &&
                n.Nam == input.Nam);

            if (duplicate)
                ModelState.AddModelError("", "Danh mục này đã có ngân sách trong tháng/năm đã chọn.");

            if (ModelState.IsValid)
            {
                existing.MaDanhMuc = input.MaDanhMuc;
                existing.Thang = input.Thang;
                existing.Nam = input.Nam;
                existing.HanMuc = input.HanMuc;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật ngân sách thành công!";
                return RedirectToAction(nameof(Index), new
                {
                    thang = existing.Thang,
                    nam = existing.Nam
                });
            }

            await LoadExpenseCategories(userId.Value, input.MaDanhMuc);
            return View("~/Views/NganSach/Edit.cshtml", input);
        }

        // POST: /NganSach/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, byte? thang, short? nam)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var budget = await _context.NganSach.FirstOrDefaultAsync(n =>
                n.MaNganSach == id && n.UserId == userId.Value);

            if (budget != null)
            {
                _context.NganSach.Remove(budget);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa ngân sách khỏi hệ thống.";
            }

            return RedirectToAction(nameof(Index), new
            {
                thang = thang ?? (byte)DateTime.Today.Month,
                nam = nam ?? (short)DateTime.Today.Year
            });
        }

        private async Task LoadExpenseCategories(int userId, int? selectedId = null)
        {
            var categories = await _context.DanhMuc
                .AsNoTracking()
                .Where(d => d.UserId == userId && d.Loai == "Chi")
                .OrderBy(d => d.Ten)
                .ToListAsync();

            ViewBag.MaDanhMuc = new SelectList(
                categories, "Id", "Ten", selectedId);
        }
    }
}
