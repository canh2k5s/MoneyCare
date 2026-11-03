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
            return View();
        }

        public IActionResult NganSach()
        {
            return View();
        }

        public IActionResult TietKiem()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}