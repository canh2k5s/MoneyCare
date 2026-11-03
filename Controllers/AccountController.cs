
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers
{
    public class AccountController : Controller
    {
        private readonly MoneyCareContext _context;

        public AccountController(MoneyCareContext context)
        {
            _context = context;
        }

        // Trang đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            NguoiDung user, string ConfirmPassword)
        {
            // Kiểm tra xác nhận mật khẩu
            if (user.Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View(user);
            }

            // Kiểm tra tài khoản đã tồn tại
            bool exists = await _context.NguoiDung.AnyAsync(x =>
                x.Email == user.Email ||
                x.UserName == user.UserName);

            if (exists)
            {
                ViewBag.Error = "Email hoặc tên đăng nhập đã tồn tại!";
                return View(user);
            }

            // Lưu tài khoản vào SQL Server
            user.NgayTao = DateTime.Now;

            _context.NguoiDung.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // Trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string UserName, string Password)
        {
            // Tìm tài khoản và kiểm tra mật khẩu
            var user = await _context.NguoiDung.FirstOrDefaultAsync(x =>
                (x.UserName == UserName || x.Email == UserName)
                && x.Password == Password);

            if (user == null)
            {
                ViewBag.Error =
                    "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }

            // Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("HoTen", user.HoTen);

            // Chuyển về trang chủ
            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
