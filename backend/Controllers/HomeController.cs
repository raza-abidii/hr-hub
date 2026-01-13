using EMSSolution.DataAccess;
using EMSSolution.DatabaseAccessLayer;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using Department = EMSSolution.Models.Department;
namespace EMSSolution.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        public string? UserName { get; set; } = string.Empty;
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        TableCreation TableCreation;
        string strQry = string.Empty;
        string strErrMessage = string.Empty; 
        DataLayer dl = new DataLayer();
        string database = string.Empty;
        public HomeController(ApplicationDBContext db, IUserActivityLogger userActivityLogger)
        {
            _db = db;
            _userActivityLogger = userActivityLogger;
            
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public IActionResult Index()
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

                //TableCreation.createTable();

                UserName = HttpContext.Session.GetString("UserName");
                List<Department> departments = _db.Departments.ToList();
                ViewBag.Department = departments;
                List<Shift> shifts = _db.Shifts.ToList();
                ViewBag.Shift = shifts;

                //List<AttendanceDashboardModel> attendanceDashboardModels = _db.Database.SqlQuery<AttendanceDashboardModel>
                //    ($@"exec sp_AttendanceDashBoard {DateTime.Today.Day-3}, {DateTime.Today.Month},
                //    {DateTime.Today.Year}").ToList();

                string strBranchList = HttpContext.Session.GetString("BranchList") ?? "";
                FormattableString  strQry = $@"exec sp_DailyAttendanceDashBoard {DateTime.Today.Day}, {DateTime.Today.Month},{DateTime.Today.Year}";
                List<AttendanceDashboardModel> attendanceDashboardModels = _db.Database.SqlQuery<AttendanceDashboardModel>
                   (strQry).ToList();

                if (strBranchList != "")
                    attendanceDashboardModels = attendanceDashboardModels
                        .Where(a => strBranchList.Split(',').Contains(a.iBranch.ToString()))
                        .ToList();


                ViewBag.AttendanceDashBoard = attendanceDashboardModels;

                var preferences = _db.Preferences.FirstOrDefault();
                int iSecondLevelAppprovalAuthority = 0;
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                }


                int userid = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

                #region Sub-ordinate Employee Leave Application Alert
                //Leave Alert

                List<LeaveAlert>? leaveAlert = new List<LeaveAlert>();
                if (iSecondLevelAppprovalAuthority != userid && userid > 0)
                    leaveAlert = _db.Database.SqlQuery<LeaveAlert>
                        ($@"select e.sEmployeeName	EmployeeName,format(la.dFromDate,'dd-MM-yyyy') leaveDate,
                        ld.sLeaveCode LeaveType,la.sRemarks Remarks,la.id LeaveId,'Pending' LeaveStatus
                        from tblLeaveApplication la 
                        join tblEmployee e on e.iMasterId=la.iEmployee
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType
                        where e.bEmployeeResign=0 and  la.iApproved1=0 
                        and e.iReportingTo={HttpContext.Session.GetString("EmployeeId")}").ToList();
                else
                    if (iSecondLevelAppprovalAuthority != 0)
                    leaveAlert = _db.Database.SqlQuery<LeaveAlert>
                        ($@"select e.sEmployeeName	EmployeeName,format(la.dFromDate,'dd-MM-yyyy') leaveDate,
                        ld.sLeaveCode LeaveType,la.sRemarks Remarks,la.id LeaveId,
                        case when iApproved1=1 then 'Approved by RM' else 'Pending From RM' end LeaveStatus
                        from tblLeaveApplication la 
                        join tblEmployee e on e.iMasterId=la.iEmployee
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType
                        where e.bEmployeeResign=0 and  (la.iApproved1=0 
                        and e.iReportingTo={HttpContext.Session.GetString("EmployeeId")})
                        or (la.iApproved2=0 and la.iApprovedAuthority2={iSecondLevelAppprovalAuthority})").ToList();
                ViewBag.LeaveAlert = leaveAlert;
                #endregion

                #region Self Leave Status
                if (userid > 0)
                {
                    DateTime fromdt = DateTime.Now.AddDays(-30);
                    List<LeaveStatusReport> leaveStatusReports = _db.Database.SqlQuery<LeaveStatusReport>
                        ($@"exec sp_getEmployeeLeaveStatusDB {userid},{fromdt}").ToList();


                    if (preferences != null)
                    {
                        iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                    }
                    string strSql = "", strErrMess = "";
                    if (iSecondLevelAppprovalAuthority > 0)
                    {
                        foreach (var lsr in leaveStatusReports)
                        {
                            strSql = $@"select case when iApproved2=0 then 'Pending' 
                                when iApproved2=-1 then 'Rejected' else 'Approved' 
                                end finalstatus
                                from tblLeaveApplication where id={lsr.Leaveid} 
                                and iApprovedAuthority2<>0  ";
                            DataSet ds = dl.GetData(strSql, ref strErrMess);
                            if (lsr.Status == "Approved" && Convert.ToString(ds.Tables[0].Rows[0]["finalstatus"]) == "Approved")
                            {
                                lsr.Status = "Approved";
                                lsr.ApprovedRejectedBy = lsr.ApprovedRejectedBy + " => HR";
                            }
                            else if (lsr.Status == "Approved" && Convert.ToString(ds.Tables[0].Rows[0]["finalstatus"]) == "Rejected")
                            {

                                lsr.ApprovedRejectedBy = "Approved by Manager, rejected by HR";
                            }
                            else if (lsr.Status == "Approved" && Convert.ToString(ds.Tables[0].Rows[0]["finalstatus"]) == "Pending")
                            {

                                lsr.ApprovedRejectedBy = "Approved by Manager, Pending from HR";
                            }
                            else if (lsr.Status == "Rejected" && Convert.ToString(ds.Tables[0].Rows[0]["finalstatus"]) == "Rejected")
                            {
                                lsr.ApprovedRejectedBy = "Rejected from HR";
                            }
                        }
                    }
                    ViewBag.EmployeeLeaveStatus = leaveStatusReports;
                }
                #endregion

                #region Expense Alert
                List<expenseAlertEmployee> expenseAlertEmployees = _db.Database.SqlQuery<expenseAlertEmployee>
                    ($@"select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) Sno,
                    Description,Remarks,Amount RequestedAmount,
                    case when ApprovalStatus=-1 then 'Rejected'
                    when ApprovalStatus=0 then 'Pending'
                    else 'Approved' end ApprovalStatus
                    ,ApprovedAmount 
                    from tblExpenses where iEmployee={HttpContext.Session.GetString("EmployeeId")}").ToList();
                ViewBag.expenseAlert = expenseAlertEmployees;
                #endregion

                #region Expense Alter Admin
                List<expenseAlertAdmin> expenseAlertAdmin = _db.Database.SqlQuery<expenseAlertAdmin>
                   ($@"select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) Sno,
                    e.sEmployeeName EmployeeName,
                    format( ex.ExpenseDate,'dd-MM-yyyy') ExpenseDate,
                    Description,Amount RequestedAmount,ex.id
                    from tblExpenses ex join tblEmployee e 
                    on e.iMasterId=ex.iEmployee where e.bEmployeeResign=0 and  ApprovalStatus=0").ToList();
                ViewBag.expenseAlertAdmin = expenseAlertAdmin;
                #endregion

                ViewBag.HideSideNav = false;

                ViewBag.UserName = HttpContext.Session.GetString("UserName");

                TempData["UserName"] = HttpContext.Session.GetString("UserName");

                EMSUsers? user = _db.Users.FirstOrDefault(
                              a => a.UserName == Convert.ToString(TempData["UserName"]));
                if (user != null)
                {
                    ViewBag.sImage = user?.sImage;
                    ViewBag.Role = user?.Role;

                    HttpContext.Session.SetString("sImage", user.sImage ?? "");
                    //if user.sImage is null then set sImage to ""
                }

                #region Employee Count/ Present / Absent

                var empCount = _db.Employees.Count(a=>a.bEmployeeResign==false);

                #region total Employee Basic Details

                List<employeeDetailDB> AllEmpDetail = (
                    from e in _db.Employees
                    join c in _db.Categories on e.iCategory equals c.iMasterid into categoryGroup
                    from c in categoryGroup.DefaultIfEmpty()
                    join d in _db.Departments on e.iDepartment equals d.iMasterid into deptGroup
                    from d in deptGroup.DefaultIfEmpty()
                    join desig in _db.Designations on e.iDesignation equals desig.iMasterid into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    join br in _db.Branches on e.iBranch equals br.iMasterid into branchGroup
                    from br in branchGroup.DefaultIfEmpty()
                    where e.bEmployeeResign == false
                    orderby br.sName
                    select new employeeDetailDB
                    {
                        EmpName = e.sEmployeeName,
                        EmpId = e.sEmployeeCode,
                        Branch = br != null ? br.sName : null,
                        BranchId= br != null ? br.iMasterid : 0,
                        Category = c != null ? c.sName : null,
                        Department = d != null ? d.sName : null,
                        Designation = desig != null ? desig.sName : null
                    }).ToList();

                if (strBranchList != "")
                    AllEmpDetail = AllEmpDetail
                        .Where(a => strBranchList.Split(',').Contains(a.BranchId.ToString()))
                        .ToList();

                ViewBag.AllEmpDetail = AllEmpDetail;

                ViewBag.empCount = AllEmpDetail.Count();
                #endregion

                DateTime targetDate = DateTime.Now;

                //var empPresentCount = (from es in _db.EmployeeTimeSheets
                //    join e in _db.Employees on es.EmpId equals e.sEmployeeCode
                //    where es.LogDateTime.Date == targetDate.Date
                //    select es.EmpId)
                //    .Distinct()
                //    .Count();

                //ViewBag.empPresentCount = empPresentCount;
                ViewBag.empPresentCount = attendanceDashboardModels.Count();

                #region Absent Employee Basic Detail
                ////List<employeeDetailDB> AbsentEmployeeDetail = (
                ////    from e in _db.Employees
                ////    join c in _db.Categories on e.iCategory equals c.iMasterid into categoryGroup
                ////    from c in categoryGroup.DefaultIfEmpty()
                ////    join d in _db.Departments on e.iDepartment equals d.iMasterid into deptGroup
                ////    from d in deptGroup.DefaultIfEmpty()
                ////    join desig in _db.Designations on e.iDesignation equals desig.iMasterid into desigGroup
                ////    from desig in desigGroup.DefaultIfEmpty()
                ////    join br in _db.Branches on e.iBranch equals br.iMasterid into branchGroup
                ////    from br in branchGroup.DefaultIfEmpty()
                ////    where !_db.EmployeeTimeSheets
                ////    .Where(t => t.LogDateTime.Day == DateTime.Now.Day &&
                ////                t.LogDateTime.Month == DateTime.Now.Month &&
                ////                t.LogDateTime.Year == DateTime.Now.Year)
                ////    .Select(t => t.EmpId)
                ////    .Distinct()
                ////    .Contains(e.sEmployeeCode)
                ////    orderby br.sName
                ////    select new employeeDetailDB
                ////    {
                ////        EmpName = e.sEmployeeName,
                ////        EmpId = e.sEmployeeCode,
                ////        Branch = br != null ? br.sName : null,
                ////        BranchId = br != null ? br.iMasterid : 0,
                ////        Category = c.sName,
                ////        Department = d.sName,
                ////        Designation = desig.sName
                ////    }).ToList();

                List<employeeDetailDB> AbsentEmployeeDetail= new List<employeeDetailDB>();
                var presentEmpCodes = attendanceDashboardModels
                    .Select(a => a.eMasterid)
                    .Distinct()
                    .ToList();
                AbsentEmployeeDetail = (
                    from e in _db.Employees
                    join c in _db.Categories on e.iCategory equals c.iMasterid into categoryGroup
                    from c in categoryGroup.DefaultIfEmpty()
                    join d in _db.Departments on e.iDepartment equals d.iMasterid into deptGroup
                    from d in deptGroup.DefaultIfEmpty()
                    join desig in _db.Designations on e.iDesignation equals desig.iMasterid into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    join br in _db.Branches on e.iBranch equals br.iMasterid into branchGroup
                    from br in branchGroup.DefaultIfEmpty()
                    where !presentEmpCodes.Contains(e.iMasterid)
                    && e.bEmployeeResign==false
                    orderby br.sName
                    select new employeeDetailDB
                    {
                        EmpName = e.sEmployeeName,
                        EmpId = e.sEmployeeCode,
                        Branch = br != null ? br.sName : null,
                        BranchId = br != null ? br.iMasterid : 0,
                        Category = c != null ? c.sName : null,
                        Department = d != null ? d.sName : null,
                        Designation = desig != null ? desig.sName : null
                    }).ToList();

                if (strBranchList != "")
                    AbsentEmployeeDetail = AbsentEmployeeDetail
                        .Where(a => strBranchList.Split(',').Contains(a.BranchId.ToString()))
                        .ToList();

                ViewBag.AbsentEmployeeDetail = AbsentEmployeeDetail;
                //ViewBag.AbsentEmployeeCount = AbsentEmployeeDetail.Count();

                ViewBag.AbsentEmployeeCount= empCount - ViewBag.empPresentCount;
                #endregion

                #region EmployeeLatein
                var lateInList = attendanceDashboardModels
                   .Where(x => x.LateINEarlyOut == "Late-In")
                   .ToList();
                ViewBag.empLatein = lateInList.Count();
                ViewBag.LateinEmployeeDetail = lateInList;
                #endregion

                #region EmployeeEarlyOut
                var EarlyOutList = attendanceDashboardModels
                   .Where(x => x.LateINEarlyOut == "Early Out"
                    && x.InTime.ToString("HH:mm")!=x.OutTime.ToString("HH:mm"))
                   .ToList();
                ViewBag.empEarlyout = EarlyOutList.Count();
                ViewBag.EarlyOutEmployeeDetail = EarlyOutList;
                #endregion

                #endregion

                #region Timecard Detail for Dashboard

                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                ViewBag.Year = year;
                ViewBag.Month = month;
                int iEmployee = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                GenericFunction.WriteLog("ControllerHitting", $@"exec spTimeCard {iEmployee}, {month}, {year}");
                List<Timecard> timecards = _db.Database.SqlQuery<Timecard>
                    ($@"exec sp_TimeCard {iEmployee}, {month}, {year}").ToList();

                GenericFunction.WriteLog("ControllerHitting", $@"timecard data total rows: " + timecards.Count);
                List<TimecardAdvance> timecardAdvances = getEmployeeTimecard(month, year);
                #endregion

                #region Get Last Week Chart
                List<EmployeeLastWeekAbsentPresentChart> EmployeeLastWeekDetail = _db.Database.SqlQuery<EmployeeLastWeekAbsentPresentChart>
                   ($@"WITH Last7Days AS
                    (
                        SELECT CAST(GETDATE() AS date) AS LogDate
                        UNION ALL
                        SELECT DATEADD(DAY, -1, cast(LogDate as date))
                        FROM Last7Days
                        WHERE cast(LogDate as date) > DATEADD(DAY, -6, CAST(GETDATE() AS date))
                    )
                    SELECT 
                        CAST(d.LogDate AS varchar(12)) AS dDate,
                        (SELECT COUNT(*) FROM tblEmployee where bEmployeeResign=0) AS TotalEmployees,
                        COUNT(DISTINCT  case when isnull(e.sEmployeeCode,'')<>'' then ts.EmpId end) AS PresentEmployees,
                        (SELECT COUNT(*) FROM tblEmployee where bEmployeeResign=0) - 
	                    COUNT(DISTINCT case when isnull(e.sEmployeeCode,'')<>'' then ts.EmpId end) AS AbsentEmployees
                        FROM Last7Days d
                        LEFT JOIN tblEmployeeTimeSheet ts 
                        ON CAST(ts.logDateTime AS date) = d.LogDate
                        left join tblEmployee e on e.sEmployeeCode=ts.EmpId and isnull(e.bEmployeeResign,0)=0
                        WHERE 
                        DATEPART(WEEKDAY, d.LogDate) <> 1   -- Skip Sundays (1 = Sunday if DATEFIRST=7)
                        --and CAST ( logdatetime as time) >'02:00:00'  or ts.EmpId is null 
                        GROUP BY d.LogDate
                        ORDER BY d.LogDate;
                        ").ToList();

                ViewBag.EmployeeLastWeekDetail = EmployeeLastWeekDetail;
                #endregion

                #region Weekly EmployeeAttendanceDB
                DateTime todate = DateTime.Now;
                DateTime fromdate= DateTime.Now.AddDays(-7);
                List<EmployeeLastWeekAttendanceChart> employeeLastWeekAttendanceCharts =
                _db.Database.SqlQuery<EmployeeLastWeekAttendanceChart>
                    ($@"EXEC sp_LastweekAttendanceReport @fromdate = {fromdate.ToString("yyyy-MM-dd")}, 
                    @todate = {todate.ToString("yyyy-MM-dd")}, 
                    @iEmployee = {HttpContext.Session.GetString("EmployeeId")}").ToList();

                ViewBag.EmployeeLastWeekAttendance = employeeLastWeekAttendanceCharts;
                #endregion

                #region Login Time of Employee

                var loginTime = _db.Database.SqlQuery<string>($@"
                    select top 1 LogTime value 
                    from tblEmployee e 
                    left join tblEmployeeTimeSheet ts on e.sEmployeeCode=ts.EmpId
                    where cast(logdatetime as date)=cast(getdate() as date)
                    and e.iMasterid={HttpContext.Session.GetString("EmployeeId")}
                    order by logDateTime asc").FirstOrDefault();

                if (loginTime != null)
                    ViewBag.LoginTime = Convert.ToDateTime(loginTime).ToString("HH:mm:ss");
                else
                    ViewBag.LoginTime = "";

                #endregion

                #region Employee Basic Detail
                var employeeInfo = _db.Database.SqlQuery<employeeInfo>($@"
                    select isnull(e.sImage,'') Photo, isnull(e.sEmployeeName,'') EmpName,isnull(e.sEmployeeCode,'') EmpId,
                    isnull(b.sName,'') Branch,isnull(c.sName,'') Category,
                    isnull(d.sName,'') Department,isnull(d.sName,'') Designation
                    from tblEmployee e 
                    left join tblBranch b on b.iMasterid=e.iBranch
                    left join tblCategory c on c.iMasterid=e.iCategory
                    left join tblDepartment d on d.iMasterid=e.iDepartment
                    left join tblDesignation de on de.iMasterid=e.iDesignation
                    where e.iMasterId={HttpContext.Session.GetString("EmployeeId")}
                    ").FirstOrDefault();

                if (loginTime != null)
                {
                    ViewBag.EmployeeName = Convert.ToString(employeeInfo.EmpName);
                    ViewBag.EmployeeDesignation = Convert.ToString(employeeInfo.Designation);
                    ViewBag.EmployeePhoto = Convert.ToString(employeeInfo.Photo);
                }
                else
                {
                    ViewBag.EmployeeName = "";
                    ViewBag.EmployeeDesignation = "";
                    ViewBag.EmployeePhoto = "";
                }
                #endregion

                return View(timecardAdvances);
            }
            catch (Exception ex)
            {
                // Handle the error (e.g., log it)
                TempData["ToastMessage"] = "Exception: " + ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        public IActionResult refreshDepartment()
        {
            //var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");

            //if (isAuthenticated != "true")
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            List<Department> departments = _db.Departments.ToList();
            //ViewBag.Department = departments;

            return Json(new { success = "", data = departments });
        }

        public IActionResult CompanyDetail()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult UpdateProfilePicture(IFormFile profileImage)
        {
            try
            {
                if (profileImage != null && profileImage.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        profileImage.CopyToAsync(ms);
                        var base64String = Convert.ToBase64String(ms.ToArray());

                        // Assuming you're using ASP.NET Identity

                        EMSUsers? user = _db.Users.FirstOrDefault(
                            a => a.UserName == Convert.ToString(HttpContext.Session.GetString("UserName")));


                        if (user != null)
                        {
                            user.sImage = base64String;
                            _db.Update(user);
                            _db.SaveChanges();

                            //If User is Employee then update Employee Image Too
                            if (user.iEmployee > 0)
                            {
                                Employee emp = _db.Employees.FirstOrDefault(a => a.iMasterid == user.iEmployee);
                                emp.sImage = "data:image/png;base64," + base64String;
                                _db.Update(emp);
                                _db.SaveChanges();
                            }

                            _userActivityLogger.LogAsync(
                            user.Id.ToString(),
                            user.UserName,
                            "Home",
                            "UpdateProfilePicture",
                            "Updated profile picture");
                        }

                       

                        // Log the activity
                    }
                }

                return RedirectToAction("Index", "Home"); // Or wherever appropriate
            }
            catch (Exception ex)
            {
                _userActivityLogger?.LogAsync(Convert.ToString(HttpContext.Session.GetString("UserId")),
                    Convert.ToString(HttpContext.Session.GetString("UserName")),
                    "Home", "UpdateProfilePicture", "Exeption: " + ex.Message);
                // Handle the error (e.g., log it)
                return RedirectToAction("Index", "Home"); // Or wherever appropriate
            }
        }

        [HttpPost]
        public IActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                // Validate the old password
                // Example: Check if the old password is correct
                var usr = _db.Users.FirstOrDefault(
                    a => a.UserName == Convert.ToString(HttpContext.Session.GetString("UserName")));
                //session expired
                if (usr == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                bool bUserAuthenticated = PasswordMasking.PasswordHasher.VerifyPassword(OldPassword, usr.PasswordHash, usr.Salt);
                if (!bUserAuthenticated)
                {
                    TempData["ToastMessage"] = "Old password is wrong";
                    TempData["ToastType"] = "danger";
                    TempData["ShowChangePasswordModal"] = "true"; // trigger modal
                    return RedirectToAction("Index");
                }
                if (NewPassword != ConfirmPassword)
                {
                    TempData["ToastMessage"] = "New password and confirm password do not match.";
                    TempData["ToastType"] = "danger";
                    TempData["ShowChangePasswordModal"] = "true"; // trigger modal
                    return RedirectToAction("Index");
                }

                var maskedPassword = PasswordMasking.PasswordHasher.HashPassword(NewPassword);
                usr.PasswordHash = maskedPassword.hashedPassword;
                usr.Salt = maskedPassword.salt;
                _db.Users.Update(usr);
                _db.SaveChanges();

                // Validate old password and update logic here...
                // Example: Check hashed old password with stored hash, then save new hashed password

                TempData["ToastMessage"] = "Password changed successfully!";
                TempData["ToastType"] = "success";

                // Log the activity
                _userActivityLogger.LogAsync(
                    usr.Id.ToString(),
                    usr.UserName,
                    "Home",
                    "ChangePassword",
                    "Changed password");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Exception: " + ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToAction("Index");
            }
        }


        public IActionResult TimecardAdvanced(int employeeId, int month = 0, int year = 0)
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

                if (year == 0 || month == 0)
                {
                    year = DateTime.Now.Year;
                    month = DateTime.Now.Month;
                }
                int iEmployee = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                var employeetimecard = getEmployeeTimecard(month, year);
                //ViewBag.employeeTimecard = employeetimecard;
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View("Index", employeetimecard);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        private List<TimecardAdvance> getEmployeeTimecard(int iMonth, int iYear)
        {

            try
            {
                //entity frameowrk to call store procedure
                int iEmployee = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                List<Timecard> attendanceReportModels = _db.Database.SqlQuery<Timecard>
                    ($@"exec sp_TimeCard {iEmployee}, {iMonth}, {iYear}").ToList();

                List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>(); //list of dictionary to store attendance data
                Dictionary<string, string> attendancedata = new Dictionary<string, string>();

                List<TimecardAdvance> timecardAdvances = new List<TimecardAdvance>();
                TimecardAdvance timecardAdvance = new TimecardAdvance();

                int daysInMonth = DateTime.DaysInMonth(iYear, iMonth);
                for (int i = 0; i < daysInMonth; i++)
                {
                    string sHoliday = "Absent";
                    var date = new DateTime(iYear, iMonth, i + 1);
                    //check if date is sunday
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        sHoliday = "Public Holiday";
                    }

                    Timecard? attendance = attendanceReportModels.Where(x => x.LogDate == date.ToString("dd-MM-yyyy")).FirstOrDefault();
                    timecardAdvance = new TimecardAdvance();
                    if (attendance == null)
                    {
                        //string currentDate = date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        ////check for Leave on selected Date
                        //string sql = $@"
                        //    SELECT fTotalDaysTaken 
                        //    FROM tblLeaveApplication 
                        //    WHERE iEmployee = {iEmployee} 
                        //    AND '{currentDate}' BETWEEN dFromDate AND dToDate";

                        //var leaveTaken = _db.Database.SqlQuery<LeaveResponse>(FormattableStringFactory.Create(sql))
                        //        .ToList();

                        timecardAdvance.HoursWorked = DateTime.Now.TimeOfDay;
                        timecardAdvance.InTime = DateTime.Now;
                        timecardAdvance.OutTime = DateTime.Now;
                        timecardAdvance.LogDate = date.ToString("dd-MM-yyyy");
                        timecardAdvance.Remakrs = sHoliday;
                    }
                    else
                    {
                        if (attendance.status == "Leave")
                        {
                            timecardAdvance.HoursWorked = DateTime.Now.TimeOfDay;
                            timecardAdvance.InTime = DateTime.Now;
                            timecardAdvance.OutTime = DateTime.Now;
                            timecardAdvance.LogDate = date.ToString("dd-MM-yyyy");
                            timecardAdvance.Remakrs = "Leave";
                        }
                        else
                        {
                            var result = _db.Database.SqlQuery<ShiftTimeResult>($@"
                                select dateadd(second,59, cast(cast(SinTime as time) as datetime)) sintime ,
                                cast(cast(SoutTime as time) as datetime) SoutTime 
                                from (
                                SELECT 
                                DATEADD(minute, s.iAllowlateminute, s.sStartTime) AS SinTime,
                                DATEADD(minute, s.iAllowearlyminute, s.sEndTime) AS SoutTime
                                FROM tblShiftDefinition s
                                JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift
                                WHERE sa.iDay = {attendance.InTime.Day} AND sa.iMonth = {iMonth}
                                AND sa.iEmployee = {iEmployee} AND sa.iYear = {iYear}
                                )t
                        ").ToList();

                            if (result.Count > 0)
                            {
                                var shiftTime = result[0];


                                if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay
                                    && attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                {
                                    timecardAdvance.Remakrs = "Late In/Early Out";

                                }
                                else if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay)
                                {
                                    timecardAdvance.Remakrs = "Late In";

                                }
                                else if (attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                {
                                    timecardAdvance.Remakrs = "Early Out";

                                }
                                else
                                {
                                    timecardAdvance.Remakrs = "On Time";

                                }
                            }
                            else
                            {
                                timecardAdvance.Remakrs = "";
                            }

                            timecardAdvance.InTime = attendance.InTime;
                            timecardAdvance.OutTime = attendance.OutTime;
                            timecardAdvance.LogDate = attendance.LogDate;
                            timecardAdvance.HoursWorked = attendance.HoursWorked;
                            //attendancedata.Add("day-" + i + "-in", attendance.InTime.ToString("HH:mm"));
                            //attendancedata.Add("day-" + i + "-out", attendance.OutTime.ToString("HH:mm"));
                            //attendancedata.Add("day-" + i + "-ph", sHoliday);
                            //attendancedata.Add("LogDate-" + i , date.ToString("dd-MM-yyyy")); 
                        }
                    }
                    timecardAdvances.Add(timecardAdvance);
                }


                return (timecardAdvances);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
