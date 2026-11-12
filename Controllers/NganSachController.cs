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
            return HttpContext.Session.GetInt32("UserId") ?? 1; 
        }

        // GET: NganSach
        public async Task<IActionResult> Index(byte? thang, short? nam)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var currentDate = DateTime.Now;
            byte selectedMonth = thang ?? (byte)currentDate.Month;
            short selectedYear = nam ?? (short)currentDate.Year;

            ViewBag.Thang = selectedMonth;
            ViewBag.Nam = selectedYear;

            var listNganSach = await _context.VwTheoDoiNganSachs
                .Where(x => x.UserId == userId && x.Thang == selectedMonth && x.Nam == selectedYear)
                .ToListAsync();

            return View(listNganSach);
        }

        // GET: NganSach/Create
        public async Task<IActionResult> Create()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMuc.Where(d => d.UserId == userId && d.Loai == "Chi").ToListAsync(), 
                "Id", "Ten"
            );

            var model = new NganSach
            {
                Thang = (byte)DateTime.Now.Month,
                Nam = (short)DateTime.Now.Year
            };
            return View(model);
        }

        // POST: NganSach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDanhMuc,Thang,Nam,HanMuc")] NganSach nganSach)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            nganSach.UserId = userId.Value;

            bool exists = await _context.NganSach.AnyAsync(n => 
                n.UserId == userId && n.MaDanhMuc == nganSach.MaDanhMuc && n.Thang == nganSach.Thang && n.Nam == nganSach.Nam);

            if (exists)
            {
                ModelState.AddModelError("", "Ngân sách cho danh mục này trong tháng/năm đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.NganSach.Add(nganSach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thiết lập ngân sách thành công!";
                return RedirectToAction(nameof(Index), new { thang = nganSach.Thang, nam = nganSach.Nam });
            }

            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMuc.Where(d => d.UserId == userId && d.Loai == "Chi").ToListAsync(), 
                "Id", "Ten", nganSach.MaDanhMuc
            );
            return View(nganSach);
        }

        // GET: NganSach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            int? userId = GetCurrentUserId();
            var nganSach = await _context.NganSach.FirstOrDefaultAsync(n => n.MaNganSach == id && n.UserId == userId);
            
            if (nganSach == null) return NotFound();

            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMuc.Where(d => d.UserId == userId && d.Loai == "Chi").ToListAsync(), 
                "Id", "Ten", nganSach.MaDanhMuc
            );
            return View(nganSach);
        }

        // POST: NganSach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNganSach,MaDanhMuc,Thang,Nam,HanMuc")] NganSach nganSach)
        {
            if (id != nganSach.MaNganSach) return NotFound();

            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            nganSach.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nganSach);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật ngân sách thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NganSachExists(nganSach.MaNganSach)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { thang = nganSach.Thang, nam = nganSach.Nam });
            }

            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMuc.Where(d => d.UserId == userId && d.Loai == "Chi").ToListAsync(), 
                "Id", "Ten", nganSach.MaDanhMuc
            );
            return View(nganSach);
        }

        // POST: NganSach/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? userId = GetCurrentUserId();
            var nganSach = await _context.NganSach.FirstOrDefaultAsync(n => n.MaNganSach == id && n.UserId == userId);
            
            if (nganSach != null)
            {
                _context.NganSach.Remove(nganSach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa ngân sách thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NganSachExists(int id)
        {
            int? userId = GetCurrentUserId();
            return _context.NganSach.Any(e => e.MaNganSach == id && e.UserId == userId);
        }
    }
}