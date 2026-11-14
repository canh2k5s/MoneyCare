using Microsoft.AspNetCore.Mvc;

namespace MoneyCare.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GiaoDich()
        {
            // Trỏ thẳng đến file trong thư mục Views/GiaoDich
            return View("~/Views/GiaoDich/GiaoDich.cshtml");
        }

        public IActionResult NganSach()
        {
            // Trỏ thẳng đến file trong thư mục Views/NganSach
            return View("~/Views/NganSach/NganSach.cshtml");
        }

        public IActionResult TietKiem()
        {
            // File này bạn đã có sẵn trong Views/Home/TietKiem.cshtml
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}