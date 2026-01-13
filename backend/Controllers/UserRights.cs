using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class UserRights : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger _userActivityLogger;
        string database = string.Empty;
        public UserRights(ApplicationDBContext dBContext,IUserActivityLogger userActivityLogger)
        {
            _db = dBContext;
            _userActivityLogger = userActivityLogger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        [HttpPost]
        public IActionResult SaveUserRights(string role, List<string> rights)
        { 
            try
            {
                var isExist = _db.userRights.FirstOrDefault(c => c.Role == role);
                if (isExist!= null)
                {
                    _db.userRights.Remove(isExist);
                }
                foreach (var right in rights)
                {
                    var userRight = new Models.UserRights
                    {
                        Role = role,
                        Menuitem = right
                    };
                    _db.userRights.Add(userRight);
                }
                _db.SaveChanges();
                // Log the user activity
                _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                , HttpContext.Session.GetString("UserName")
                , "UserRights", "SaveUserRights", "rights assigned");

                return Json(new { status = true, message = "User rights saved successfully.", isNew = 1, data = "" });
            }
            catch(Exception ex)
            {
                // Log the exception
                return Json(new { status = false, message = "An error occurred while saving user rights.", isNew = 0, data = "" });
            }
        }

        [HttpGet]
        public IActionResult GetRightsByRole(string role)
        {
            var rights = _db.userRights
                .Where(r => r.Role == role)
                .Select(r => r.Menuitem)
                .ToList();

            return Json(rights);
        }

        [HttpPost]
        public IActionResult DeleteUserRights(string role)
        {
            try
            {
                var userRights = _db.userRights.Where(c => c.Role == role).ToList();
                if (userRights != null && userRights.Count > 0)
                {
                    _db.userRights.RemoveRange(userRights);
                    _db.SaveChanges();
                    _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                       , HttpContext.Session.GetString("UserName")
                       , "UserRights", "DeleteUserRights", "Rights Deleted");

                    return Json(new { status = true, message = "User rights deleted successfully.", isNew = 1, data = "" });
                }
                else
                {
                    return Json(new { status = false, message = "No user rights found for the specified role.", isNew = 0, data = "" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { status = false, message = "An error occurred while deleting user rights.", isNew = 0, data = "" });
            }
        }

    }
}
