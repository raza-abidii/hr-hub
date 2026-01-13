using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class LeaveController : Controller
    {
        readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public LeaveController(ApplicationDBContext db,IUserActivityLogger userActivityLogger)
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
        public async Task< IActionResult> LeavePage()
        {
            try
            {
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Account", "Login");
                }

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                    HttpContext.Session.GetString("UserName")
                    , "Leave", "LeavePage", "user opened Leave definition page");

                List<Leave> leaves = _db.Leaves.ToList();
                return View(leaves);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task< ActionResult> SaveLeave([FromBody] Leave  leave)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                //get next masterid for new entry
                if (leave.iMasterid != 0)
                {
                    var leaveExist = _db.Leaves.FirstOrDefault(c => c.iMasterid != leave.iMasterid
                    && c.sLeaveCode == leave.sLeaveCode);
                    if (leaveExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    leaveExist = null;
                }
                else
                {
                    var leaveExist = _db.Leaves.FirstOrDefault(c => c.sLeaveCode == leave.sLeaveCode);
                    if (leaveExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    leaveExist = null;
                }

                if (leave.iMasterid == 0)
                {
                    //categoryData = new Category();
                    var isRowExist = _db.Leaves.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Leaves.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }

                    leave.iMasterid = maxMasterId;
                    _db.Leaves.Add(leave);
                    _db.SaveChanges();
                    _db.Dispose();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                       HttpContext.Session.GetString("UserName")
                       , "Leave", "SaveLeave", "user added Leave Master: " + leave.sLeaveCode);

                    strMessage = "Leave added successfully";
                }
                else
                {
                    _db.Leaves.Update(leave);
                    _db.SaveChanges();
                    iNew = 0;
                    _db.Dispose();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                       HttpContext.Session.GetString("UserName")
                       , "Leave", "SaveLeave", "user updated Leave Master: " + leave.sLeaveCode);
                    strMessage = "Leave updated successfully";
                }

                List<Leave> lstLeave = new List<Leave>();
                lstLeave.Add(leave);

                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstLeave });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

        [HttpPost]
        public async Task< ActionResult> DeleteLeave(int iMasterId)
        {
            string strMessage = "";
            try
            {
                var leaveExist = _db.Leaves.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (leaveExist != null)
                {
                    _db.Leaves.Remove(leaveExist);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                       HttpContext.Session.GetString("UserName")
                       , "Leave", "DeleteLeave", "user deleted Leave Master: " + leaveExist.sLeaveCode);

                    strMessage = "Leave deleted successfully";
                }
                else
                {
                    strMessage = "Leave not found";
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
