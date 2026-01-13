//using DocumentFormat.OpenXml.Bibliography;
//using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace EMSSolution.Controllers
{
    public class EmployeeController : Controller
    {
        readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string strQry = string.Empty;
        string database = string.Empty;
        public EmployeeController(ApplicationDBContext db, IUserActivityLogger userActivityLogger)
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

        //[HttpPost]
        //public IActionResult SaveSalary(EmployeeSalaryViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        //ViewBag.EmployeeList = new SelectList(_db.employeeSalaries, "iEmployeeId", "sEmployeeName");
        //        //return View("EmployeeSalaryDefinition", model);
        //    }

        //    // Save logic here

        //    return RedirectToAction("Index", "Employee");
        //}

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
                return View(employeetimecard);
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
                                select cast(cast(SinTime as time) as datetime) sintime ,
                                cast(cast(SoutTime as time) as datetime) SoutTime 
                                from (
                                SELECT 
                                DATEADD(minute, s.iAllowlateminute, s.sStartTime) AS SinTime,
                                DATEADD(minute, s.iAllowearlyminute, s.sEndTime) AS SoutTime
                                FROM tblShiftDefinition s
                                JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift
                                WHERE sa.iDay = {attendance.InTime.Day} AND sa.iMonth = {iMonth}
                                AND sa.iEmployee = {iEmployee} AND sa.iYear = {iYear}
                                )t").ToList();

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

        public IActionResult Timecard(int employeeId = 0, int month = 0, int year = 0)
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

                GenericFunction.WriteLog("ControllerHitting", "Timecard hits ");
                if (year == 0 || month == 0)
                {
                    year = DateTime.Now.Year;
                    month = DateTime.Now.Month;
                }
                int iEmployee = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                GenericFunction.WriteLog("ControllerHitting", $@"exec spTimeCard {iEmployee}, {month}, {year}");
                List<Timecard> timecards = _db.Database.SqlQuery<Timecard>
                    ($@"exec sp_TimeCard {iEmployee}, {month}, {year}").ToList();

                GenericFunction.WriteLog("ControllerHitting", $@"timecard data total rows: " + timecards.Count);
                ViewBag.Year = year;
                ViewBag.Month = month;
                GenericFunction.WriteLog("ControllerHitting", "Timecard Action finished ");
                return View(timecards);
            }
            catch (Exception ex)
            {
                // Handle exception
                GenericFunction.WriteLog("ControllerHitting", "Timecard Exception: " + ex.Message);
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        public IActionResult Calendar(int month = 0, int year = 0)
        {
            try
            {
                if (year == 0 || month == 0)
                {
                    year = DateTime.Now.Year;
                    month = DateTime.Now.Month;
                }
                int iEmployee = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                List<Timecard> timecards = _db.Database.SqlQuery<Timecard>
                    ($@"exec sp_TimeCard {iEmployee}, {year}, {month}").ToList();

                ViewBag.Year = year;
                ViewBag.Month = month;
                return View(timecards);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IActionResult> Leave()
        {
            try
            {
                #region check if Session is Live or Expired
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Login", "Account");
                }
                #endregion

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "Employee", "Leave", "User opened Leave Page");


                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true
                        && (a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) ||
                        a.iReportingTo == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")))).ToList();

                    ViewBag.Employees = employees;
                }

                var leave = _db.Leaves.ToList();
                ViewBag.Leave = leave;
                return View();
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IActionResult> EmployeeTimeSheetUpdate()
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
                    , "Employee", "EmployeeTimeSheetUpdate", "opened Manual Employee Time Sheet update Page");

                var strBranchList = HttpContext.Session.GetString("BranchList");

                var employees = _db.Employees.ToList();
                if (!string.IsNullOrEmpty(strBranchList))
                    employees = employees.Where(e => strBranchList.Split(',').Contains(e.iBranch.ToString())).ToList();
                ViewBag.Employees = employees;
                return View();
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IActionResult> EmployeePage()
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
                    , "Employee", "EmployeePage", "User opened Employee Page");

                //get all categories from table tblCategory
                List<Category>? categories = _db.Categories.ToList();
                categories.Add(new Category { iMasterid = 0, sName = "Select Category" });
                ViewBag.Categories = categories;

                //get all branches from table tblBranch
                var Branchlst = HttpContext.Session.GetString("BranchList");
                if (string.IsNullOrEmpty(Branchlst))
                {
                    List<Branch>? branches = _db.Branches.ToList();

                    branches.Add(new Branch { iMasterid = 0, sName = "Select Branch" });
                    ViewBag.Branches = branches;
                }
                else
                {
                    var branchIds = Branchlst.Split(',')
                         .Select(int.Parse)
                         .ToList();
                    List<Branch>? branches = _db.Branches.Where(a => branchIds.Contains(a.iMasterid)).ToList();

                    branches.Add(new Branch { iMasterid = 0, sName = "Select Branch" });
                    ViewBag.Branches = branches;
                }
                var designations = _db.Designations.ToList();
                designations.Add(new Designation { iMasterid = 0, sName = "Select Designation" });
                ViewBag.Designations = designations;
                var departments = _db.Departments.ToList();
                departments.Add(new Department { iMasterid = 0, sName = "Select Department" });
                ViewBag.Departments = departments;
                var shifts = _db.Shifts.ToList();
                shifts.Add(new Shift { iMasterid = 0, sShiftName = "Select Shift" });
                ViewBag.Shifts = shifts;
                var countries = _db.Countries.ToList();
                countries.Add(new Country { Id = 0, CountryName = "Select Country" });
                ViewBag.Countries = countries;
                var states = _db.States.ToList();
                states.Add(new State { Id = 0, StateName = "Select State", CountryName = "" });
                ViewBag.States = states;
                var cities = _db.Cities.ToList();
                cities.Add(new City { Id = 0, CityName = "Select City", StateName = "" });
                ViewBag.Cities = cities;

                var reporting = _db.Employees.ToList();
                ViewBag.Reporting = reporting;

                var leaveType = _db.Leaves.ToList();
                ViewBag.LeaveTypes = leaveType;


                List<Employee> employees = _db.Employees.ToList();
                return View(employees);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeTimeSheetUpdate(int employee, DateTime date, string inTime, string outTime)
        {
            try
            {
                GenericFunction.WriteLog("EmployeeController", "EmployeeTimeSheetUpdate Hits");

                Employee? employee1 = _db.Employees.FirstOrDefault(c => c.iMasterid == employee);
                string empCode = employee1.sEmployeeCode;
                //insert into tblEmployeeTimeSheet table
                EmployeeTimeSheet employeeTimeSheet = new EmployeeTimeSheet();
                employeeTimeSheet.IPAddress = "";
                employeeTimeSheet.EmpId = empCode;
                employeeTimeSheet.EmpName = "";
                employeeTimeSheet.LogDate = date.ToString("dd-MM-yyyy");
                employeeTimeSheet.LogTime = inTime;
                DateTime dt = Convert.ToDateTime(date.ToString("yyyy-MM-dd") + " " + inTime);
                employeeTimeSheet.LogDateTime = dt;
                _db.EmployeeTimeSheets.Add(employeeTimeSheet);
                employeeTimeSheet = new EmployeeTimeSheet();
                employeeTimeSheet.IPAddress = "";
                employeeTimeSheet.EmpId = empCode;
                employeeTimeSheet.EmpName = "";
                employeeTimeSheet.LogDate = date.ToString("dd-MM-yyyy");
                employeeTimeSheet.LogTime = outTime;
                dt = Convert.ToDateTime(date.ToString("yyyy-MM-dd") + " " + outTime);
                employeeTimeSheet.LogDateTime = dt;
                _db.EmployeeTimeSheets.Add(employeeTimeSheet);
                _db.SaveChanges();

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "Employee", "EmployeeTimeSheetUpdate", "User updated Employee Time Sheet: " +
                    "Employee: " + employee + ", Date: " + date + ", inTime: " + inTime + ", outTime: " + outTime);

                return Json(new { success = "success", message = "Saved Successfully" });
            }
            catch (Exception ex)
            {
                GenericFunction.WriteLog("EmployeeController", "EmployeeTimeSheetUpdate Exception: " + ex.Message);
                // Handle exception
                return Json(new { success = "failed", message = ex.Message });
            }
        }

        [HttpPost]
        //public async Task<IActionResult> saveEmployee([FromBody] Employee employee)
        public async Task<IActionResult> saveEmployee([FromBody] SaveEmployeeRequest saveEmployeeRequest)
        {
            int iNew = 1;
            string strMessage = "";
            int employeeMasterid = 0;
            try
            {
                if (saveEmployeeRequest == null)
                {
                    return this.Json(new { status = false, message = "Improper data", isNew = 0, data = "" });
                }

                Employee employee = saveEmployeeRequest.Employee;
                List<LeaveAllocation> leaveAllocation = saveEmployeeRequest.leaveAllocation;
                //if (!string.IsNullOrEmpty(employee.sImage))
                //{
                //    employee.sImage = employee.sImage.Substring(employee.sImage.IndexOf(",") + 1);
                //}
                //get next masterid for new entry
                if (employee == null)
                {
                    return this.Json(new { status = false, message = "null in Employee data", isNew = 0, data = "" });
                }
                if (employee.iMasterid != 0)
                {
                    var employeeExist = _db.Employees.FirstOrDefault(c => c.iMasterid != employee.iMasterid
                    && c.sEmployeeCode == employee.sEmployeeCode);
                    if (employeeExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    employeeExist = null;
                }
                else
                {
                    var employeeExist = _db.Employees.FirstOrDefault(c => c.sEmployeeCode == employee.sEmployeeCode);
                    if (employeeExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    employeeExist = null;
                }

                if (employee.iMasterid == 0)
                {
                    var isRowExist = _db.Employees.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Employees.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }

                    employee.iMasterid = maxMasterId;
                    if (employee.sImage == null)
                        employee.sImage = "data:,";

                    _db.Employees.Add(employee);

                    //_db.Dispose();

                    EmployeePermanent employeePermanent = new EmployeePermanent();
                    employeePermanent.iMasterid = employee.iMasterid;
                    employeePermanent.StartDate = employee.dHireDate;
                    if (employee.bPermanent == true)
                    {
                        employeePermanent.status = "P";
                    }
                    else
                    {
                        employeePermanent.status = "T";
                    }

                    employeeMasterid = maxMasterId;

                    _db.employeePermanents.Add(employeePermanent);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Employee", "saveEmployee", "User added Employee: " + employee.sEmployeeName);

                    strMessage = "Employee added successfully";
                }
                else
                {
                    if (employee.bPermanent == true)
                    {
                        EmployeePermanent? employeeExist = _db.employeePermanents.FirstOrDefault(
                            c => c.iMasterid == employee.iMasterid && employee.bPermanent == true && c.status == "T");
                        if (employeeExist != null)
                        {
                            employeeExist.status = "P";
                            employeeExist.StartDate = DateTime.Today;
                            _db.employeePermanents.Update(employeeExist);
                        }
                    }
                    var ExistingEmployee = _db.Employees.FirstOrDefault(c => c.iMasterid == employee.iMasterid);
                    _db.Entry(ExistingEmployee).State = EntityState.Detached;
                    employee.dCreatedDate = ExistingEmployee.dCreatedDate;
                    if (employee.sImage == null)
                        employee.sImage = "data:,";

                    _db.Employees.Update(employee);
                    _db.SaveChanges();
                    iNew = 0;

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Employee", "saveEmployee", "User updated Employee: " + employee.sEmployeeName);

                    employeeMasterid = employee.iMasterid;
                    strMessage = "Employee updated successfully";
                }

                #region Leave Allocation
                if (leaveAllocation != null)
                {
                    _db.leaveAllocations.RemoveRange(_db.leaveAllocations.Where(c => c.iEmployee == employeeMasterid));
                    _db.SaveChanges();
                    foreach (var leave in leaveAllocation)
                    {
                        var leaveExist = _db.leaveAllocations.FirstOrDefault(c => c.iEmployee == employeeMasterid
                        && c.iLeaveType == leave.iLeaveType);
                        if (leaveExist != null)
                        {
                            leaveExist.sLeaveName = leave.sLeaveName;
                            _db.leaveAllocations.Update(leaveExist);
                        }
                        else
                        {
                            leave.iEmployee = employeeMasterid;
                            _db.leaveAllocations.Add(leave);
                        }
                    }
                    _db.SaveChanges();
                }
                #endregion

                List<Employee> lstEmployee = new List<Employee>();

                lstEmployee.Add(employee);
                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstEmployee });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

        [HttpGet]
        public IActionResult GetEmployeeById(int id)
        {
            try
            {
                var employee = _db.Employees.FirstOrDefault(c => c.iMasterid == id);
                var employeeLeaveAllocation = _db.leaveAllocations.Where(c => c.iEmployee == id).ToList();
                if (employee != null)
                {

                    return Json(new { status = true, message = "", data = employee, leaveAllocation = employeeLeaveAllocation });
                }
                else
                {
                    return Json(new { status = false, message = "No record found", data = "" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message, data = "" });
            }
        }

        [HttpGet]
        public JsonResult GetStatesByCountry(string countryName)
        {
            if (string.IsNullOrEmpty(countryName))
            {
                return Json(new List<State>());
            }
            var states = _db.States
                .Where(s => s.CountryName == countryName)
                .ToList();
            return Json(states);
        }

        [HttpGet]
        public JsonResult GetCityByStates(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                return Json(new List<State>());
            }
            var city = _db.Cities
                .Where(s => s.StateName == stateName)
                .ToList();
            return Json(city);
        }

        [HttpGet]
        public IActionResult DeleteEmployee(int id)
        {
            try
            {
                var employee = _db.Employees.FirstOrDefault(c => c.iMasterid == id);
                if (employee != null)
                {
                    _db.Employees.Remove(employee);
                    _db.SaveChanges();
                    _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Employee", "DeleteEmployee", "User deleted Employee: " + employee.sEmployeeName);
                    return Json(new { status = true, message = "Deleted successfully" });
                }
                else
                {
                    return Json(new { status = false, message = "No record found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
}
