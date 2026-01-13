using Microsoft.AspNetCore.Mvc;

namespace EMSSolution.Controllers
{
    public class AttendanceController : Controller
    {
        private const double OfficeLatitude = 18.21;// 17.4129152; // example: Connaught Place
        private const double OfficeLongitude = 71.10;// 78.4171008;
        private const double AllowedRadiusMeters = 200; // 200m allowed range
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Punch(string type, double lat, double lon)
        {
            double distance = GetDistanceInMeters(OfficeLatitude, OfficeLongitude, lat, lon);

            if (distance > AllowedRadiusMeters)
            {
                return Content($"❌ Punch denied. You are {distance:F0} meters away from office area.");
            }

            string punchType = type == "login" ? "Login" : "Logout";
            string msg = $"✅ {punchType} Punch successful at {DateTime.Now:hh:mm:ss tt}<br>" +
                         $"📍 Location verified within {distance:F0} meters.";

            // Optional: save in database here
            // SavePunch(UserId, punchType, lat, lon);

            return Content(msg);
        }

        private double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            var latRad1 = lat1 * Math.PI / 180;
            var latRad2 = lat2 * Math.PI / 180;
            var deltaLat = (lat2 - lat1) * Math.PI / 180;
            var deltaLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(latRad1) * Math.Cos(latRad2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
