using Microsoft.AspNetCore.Mvc;

namespace EMSSolution.Controllers
{
    public class ThemeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
