using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class HolidayController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger _userActivityLogger;
        string database = string.Empty;
        public HolidayController(ApplicationDBContext db, IUserActivityLogger userActivityLogger)
        {
            _db = db;
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
        public async Task<ActionResult> HolidayPage()
        {
            try
            {
                #region check if Session Live or Expired
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Login", "Account");
                }
                #endregion

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                    HttpContext.Session.GetString("UserName")
                    , "Holiday", "HolidayPage", "user opened Holiday Page");
                List<Holiday> holidays = _db.Holidays.ToList();
                return View(holidays);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveHoliday([FromBody] Holiday holiday)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                if (holiday.iMasterid != 0)
                {
                    var HolidayExist = _db.Holidays.FirstOrDefault(c => c.iMasterid != holiday.iMasterid
                    && c.sHolidayCode == holiday.sHolidayCode);
                    if (HolidayExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    HolidayExist = null;
                }
                else
                {
                    var HolidayExist = _db.Holidays.FirstOrDefault(c => c.sHolidayCode == holiday.sHolidayCode);
                    if (HolidayExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                }
                if (holiday.iMasterid == 0)
                {
                    int maxMasterId = 1;
                    var isRowExist = _db.Holidays.FirstOrDefault();
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Holidays.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }
                    holiday.iMasterid = maxMasterId;
                    _db.Holidays.Add(holiday);
                    iNew = 1;
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName")
                        , "Holiday", "SaveHoliday", "Holiday Master added: " + holiday.sHolidayName);
                    strMessage = "Holiday added successfully";
                }
                else
                {
                    _db.Holidays.Update(holiday);
                    iNew = 0;
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName")
                        , "Holiday", "SaveHoliday", "Holiday Master updated: " + holiday.sHolidayName);
                    strMessage = "Holiday updated successfully";
                }
                _db.SaveChanges();

                List<Holiday> lstHoliday = new List<Holiday>();
                lstHoliday.Add(holiday);
                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstHoliday });
            }
            catch (Exception ex)
            {
                strMessage = ex.Message.ToString();
            }
            return this.Json(new { status = true, message = strMessage, isNew = iNew, data = "" });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteHoliday(int iMasterId)
        {
            string strMessage = "";
            try
            {
                var holiday = _db.Holidays.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (holiday != null)
                {
                    _db.Holidays.Remove(holiday);
                    _db.SaveChanges();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName")
                        , "Holiday", "DeleteHoliday", "Holiday Master deleted: " + holiday.sHolidayName);
                    strMessage = "Holiday deleted successfully";
                }
                else
                {
                    strMessage = "Holiday not found";
                }
                return this.Json(new { status = true, message = strMessage });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }
        }

    }
}
