using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDBContext _db;
        private IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public ReportsController(ApplicationDBContext db, IUserActivityLogger userActivityLogger)
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
        public IActionResult EmployeeLeaveStatusReport()
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

                string strBranchList = HttpContext.Session.GetString("BranchList");

                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true
                        && (a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                        a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")))).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in EmployeeLeaveStatusReport: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching employees." });
            }
        }

        public IActionResult LeaveApprovalManual()
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

                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true
                        && a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"))).ToList();

                    ViewBag.Employees = employees;

                }
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in LeaveApprovalManual: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching employees." });
            }
        }

        public async Task<IActionResult> AttendanceReport()
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
                   , "Reports", "AttendanceReport", "user opened Attendance Report page");

                string strBranchList = HttpContext.Session.GetString("BranchList");

                //var employees = _db.Employees.ToList();
                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a=>a.bEmployeeResign==false).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {

                    var employees = _db.Employees.Where((a => a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                       a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) && a.bEmployeeResign ==false )).ToList();

                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }


                //Employee employees=null;
                var branches = _db.Branches.ToList();

                if (!string.IsNullOrEmpty(strBranchList))
                    branches = branches.Where(e => strBranchList.Split(',').Contains(e.iMasterid.ToString())).ToList();


                var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();

                ViewBag.selectedYear = DateTime.Now.Year;
                ViewBag.selectedMonth = DateTime.Now.Month;
                //ViewBag.Employees = employees;
                ViewBag.Branch = branches;
                ViewBag.startMonth = startMonth;
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in AttendanceReport: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching attendance data." });
            }
        }

        public async Task<IActionResult> AttendanceReportDateRange()
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
                   , "Reports", "AttendanceReport", "user opened Attendance Report page");

                string strBranchList = HttpContext.Session.GetString("BranchList");

                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bEmployeeResign == false).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where((a => a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                       a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) && a.bEmployeeResign == false)).ToList();

                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }


                //Employee employees=null;

                var branches = _db.Branches.ToList();

                if (!string.IsNullOrEmpty(strBranchList))
                    branches = branches.Where(e => strBranchList.Split(',').Contains(e.iMasterid.ToString())).ToList();

                var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();

                ViewBag.FromDate = DateTime.Now;
                ViewBag.ToDate = DateTime.Now;
                ViewBag.Branch = branches;
                ViewBag.startMonth = startMonth;
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in AttendanceReport: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching attendance data." });
            }
        }

        public async Task<IActionResult> AttendanceReportSummary()
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
                   , "Reports", "AttendanceReport", "user opened Attendance Report page");

                //var employees = _db.Employees.ToList();
                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    //var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
                    var employees = _db.Employees.Where(a => a.bEmployeeResign == false).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                        a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) && a.bEmployeeResign == false).ToList();
                    ViewBag.Employees = employees;
                }

                string strBranchList = HttpContext.Session.GetString("BranchList");


                var branch = _db.Branches.ToList();
                if (!string.IsNullOrEmpty(strBranchList))
                    branch = branch.Where(e => strBranchList.Split(',').Contains(e.iMasterid.ToString())).ToList();

                ViewBag.selectedYear = DateTime.Now.Year;
                ViewBag.selectedMonth = DateTime.Now.Month;
                //ViewBag.Employees = employees;
                ViewBag.Branch = branch;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in AttendanceReportSummary: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching attendance data." });
            }
        }

        public async Task<IActionResult> AttendanceReportVertical()
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
                   , "Reports", "AttendanceReport", "user opened Attendance Report page");

                string strBranchList = HttpContext.Session.GetString("BranchList");
                //var employees = _db.Employees.ToList();
                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bEmployeeResign == false).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where((a => a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                        a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) && a.bEmployeeResign == false)).ToList();

                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }


                var branches = _db.Branches.ToList();

                if (!string.IsNullOrEmpty(strBranchList))
                    branches = branches.Where(e => strBranchList.Split(',').Contains(e.iMasterid.ToString())).ToList();


                var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();

                ViewBag.selectedYear = DateTime.Now.Year;
                ViewBag.selectedMonth = DateTime.Now.Month;
                //ViewBag.Employees = employees;
                ViewBag.Branch = branches;
                ViewBag.startMonth = startMonth;
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in AttendanceReportVertical: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching attendance data." });
            }
        }

        public IActionResult EmployeeMasterReport()
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

                string strBranchList = HttpContext.Session.GetString("BranchList");

                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a=>a.bEmployeeResign ==false).ToList();
                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true && a.bEmployeeResign==false
                        && (a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                        a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")))).ToList();

                    if (!string.IsNullOrEmpty(strBranchList))
                        employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                    ViewBag.Employees = employees;
                }

                var branches = _db.Branches.ToList();

                if (!string.IsNullOrEmpty(strBranchList))
                    branches = branches.Where(e => strBranchList.Split(',').Contains(e.iMasterid.ToString())).ToList();
                ViewBag.Branch = branches;
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in EmployeeLeaveStatusReport: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching employees." });
            }
        }

       
    }
}
