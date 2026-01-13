using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        public string? UserName { get; set; } = string.Empty;
        string database = string.Empty;
        public IActionResult Index()
        {
            return View();
        }
        public DepartmentController(ApplicationDBContext db, IUserActivityLogger? userActivityLogger)
        {
            _db = db;
            _userActivityLogger = userActivityLogger;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        public async Task<ActionResult> DepartmentPage()
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
                    , "Department", "DepartmentPage", "User opened Department Page");

                UserName = HttpContext.Session.GetString("UserName");
                List<Department> departments = _db.Departments.ToList();
                return View(departments);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveDepartment([FromBody] Department department)
        {
            try
            {
                int iNew = 1;
                string strMessage = "";
                if (department.iMasterid != 0)
                {
                    var departmentExist = _db.Departments.FirstOrDefault(c => c.iMasterid != department.iMasterid && c.sCode == department.sCode);
                    if (departmentExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    departmentExist = null;
                }
                else
                {
                    var departmentExist = _db.Departments.FirstOrDefault(c => c.sCode == department.sCode);
                    if (departmentExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    departmentExist = null;
                }

                if (department.iMasterid == 0)
                {
                    var isRowExist = _db.Departments.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Departments.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }


                    department.iMasterid = maxMasterId;
                    _db.Departments.Add(department);
                    _db.SaveChanges();
                    //_db.Dispose();

                    strMessage = "Department added successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName"), "Department", "SaveDepartment", 
                        "User Saved Department: " + department.sCode);

                }
                else
                {

                    _db.Departments.Update(department);
                    _db.SaveChanges();
                    iNew = 0;
                    strMessage = "Department updated successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName"), "Department", "SaveDepartment", 
                        "User Updated Department: " + department.sCode);

                }
                List<Department> lstDepartment = new List<Department>();
                lstDepartment.Add(department);


                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstDepartment });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }

        }

        [HttpPost]
        public async Task<ActionResult> DeleteDepartment(int iMasterId)
        {
            try
            {
                var department = _db.Departments.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (department != null)
                {
                    var empExist = _db.Employees.FirstOrDefault(c => c.iDepartment == iMasterId);
                    if (empExist != null)
                    {
                        return this.Json(new { status = false, message = "Department Already mapped to Employee, can not delete" });
                    }

                    _db.Departments.Remove(department);
                    _db.SaveChanges();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"), 
                        HttpContext.Session.GetString("UserName"), "Department", "DeleteDepartment",
                       "User Deleted Department: " + department.sCode);
                    return this.Json(new { status = true, message = "Department deleted successfully", isNew = 0, data = "" });
                }
                else
                {
                    return this.Json(new { status = false, message = "Department not found", isNew = 0, data = "" });
                }
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

    }
}
