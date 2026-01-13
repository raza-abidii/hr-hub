using Microsoft.AspNetCore.Mvc;

namespace EMSSolution.Controllers
{
    public class GeoLocationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewLocation()
        {
            // This action can be used to display the user's current location
            // You can implement logic here to retrieve and display the location
            //test comment
            return View();
        }
    }
}
