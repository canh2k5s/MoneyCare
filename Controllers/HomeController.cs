using Microsoft.AspNetCore.Mvc;

namespace MoneyCare.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult GiaoDich()
        {
            return View("~/Views/GiaoDich/GiaoDich.cshtml");
        }

        public IActionResult NganSach()
        {
            // Chuyển sang NganSachController để truy vấn dữ liệu từ SQL Server.
            return RedirectToAction("Index", "NganSach");
        }

        public IActionResult TietKiem() => View();

        public IActionResult Privacy() => View();
    }
}
