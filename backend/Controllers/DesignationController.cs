using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class DesignationController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public IActionResult Index()
        {
            return View();
        }

        public DesignationController(ApplicationDBContext db,IUserActivityLogger userActivityLogger)
        {
            _db = db;
            _userActivityLogger = userActivityLogger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }

        public async Task<ActionResult> DesignationPage()
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

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                   , "Designation", "DesignationPage", "User opened Designation Page");

                List<Models.Designation> designations = _db.Designations.ToList();
                return View(designations);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        [HttpPost]
        public async Task< ActionResult> SaveDesignation([FromBody] Models.Designation designation)
        {
            try
            {
                int iNew = 1;
                string strMessage = "";
                if (designation.iMasterid != 0)
                {
                    var designationExist = _db.Designations.FirstOrDefault(c => c.iMasterid != designation.iMasterid && c.sCode == designation.sCode);
                    if (designationExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    designationExist = null;
                }
                else
                {
                    var designationExist = _db.Designations.FirstOrDefault(c => c.sCode == designation.sCode);
                    if (designationExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    designationExist = null;
                }
                
                if (designation.iMasterid != 0)
                {
                    _db.Designations.Update(designation);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Designation", "SaveDesignation", "User updated designation: " + designation.sName);

                    strMessage = "Designation updated successfully";
                    iNew = 0;
                }
                else
                {
                   
                    var isRowExist = _db.Designations.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Designations.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }
                   
                    designation.iMasterid = maxMasterId;
                    _db.Designations.Add(designation);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Designation", "SaveDesignation", "User created designation: " + designation.sName);

                    strMessage = "Designation added successfully";
                }
                List<Designation> lstDesignation = new List<Designation>();
                lstDesignation.Add(designation);
                return this.Json(new { status = true, message = strMessage, isNew=iNew, data = lstDesignation });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew=0, data = "" });
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteDesignation(int iMasterId)
        {
            try
            {
                var designationData = _db.Designations.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (designationData != null)
                {
                    var empExist = _db.Employees.FirstOrDefault(c => c.iDesignation == iMasterId);
                    if (empExist != null)
                    {
                        return this.Json(new { status = false, message = "Designation Already mapped to Employee, can not delete" });
                    }

                    _db.Designations.Remove(designationData);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Designation", "DeleteDesignation", "User deleted designation: " + designationData.sName);
                    return this.Json(new { status = true, message = "Designation deleted successfully", data = "" });
                }
                else
                {
                    return this.Json(new { status = false, message = "Designation not found", data = "" });
                }
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, data = "" });
            }
        }

    }
}
