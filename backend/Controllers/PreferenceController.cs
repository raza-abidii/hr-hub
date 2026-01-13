using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.CodeDom;

namespace EMSSolution.Controllers
{
    public class PreferenceController : Controller
    {
        readonly ApplicationDBContext _db;
        string database = string.Empty;
        public IActionResult Index()
        {
            return View();
        }
        public PreferenceController(ApplicationDBContext context)
        {
            _db = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        public IActionResult PreferencePage()
        {
            //var user = _db.Users.Select(a => new
            //{
            //    Id= a.Id,
            //    UserName=a.UserName
            //})
            //    .ToList();
            var user = _db.Users.ToList();
            ViewBag.user = user;
            var pref = _db.Preferences.ToList();
            ViewBag.pref = pref;
            return View();
        }

        [HttpGet]
        public IActionResult GetPreference()
        {
            try
            {
                var preference = _db.Preferences.FirstOrDefault();
                if (preference == null)
                    return this.Json(new { status = false, message = "Preference Data not found" });
                else
                    return this.Json(new { status = true, message = "", data = preference });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = "Exception: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SavePreference(bool SecondLevelApproval, string SecondLevelUser, 
            string SecondLevelEmail,string hrEmailID)
        {
            try
            {
                var pref = _db.Preferences.ToList();

                if (pref != null && pref.Count>0)
                {

                    _db.Preferences.RemoveRange(pref);

                    // Update existing preference
                    //if(pref.secLvlLeaveApproval==true)
                    //{
                    //    pref.secLvlLeaveAppUser= SecondLevelUser;
                    //    pref.secLvlLeaveAppUserMail = SecondLevelEmail;

                    //}
                    //_db.Preferences.Update(pref);
                }
                else
                {
                   
                }
                Preference prefnew = new Preference();
                // Create new preference
                prefnew.secLvlLeaveApproval = SecondLevelApproval;
                prefnew.secLvlLeaveAppUser = SecondLevelUser;
                prefnew.secLvlLeaveAppUserMail = SecondLevelEmail;
                prefnew.HrEmailId = hrEmailID;  
                _db.Preferences.Add(prefnew);
                _db.SaveChanges();
                return Json(new { success = true, message = "Preference saved successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return Json(new { success = false, message = "An error occurred while saving preferences." });
            }
        }
    }
}
