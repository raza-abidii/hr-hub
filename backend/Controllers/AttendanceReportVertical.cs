using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EMSSolution.Controllers
{

    public class AttendanceReportVertical : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public AttendanceReportVertical(ApplicationDBContext db, IUserActivityLogger? userActivityLogger)
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
        public async Task<IActionResult> GetAttendanceReportData(int iEmployee, int iBranch,
          string EmployeeName, string BranchName, int iMonth, int iYear, string ReportType)
        {
            try
            {
                //if (ReportType.ToUpper() == "EmployeeWise".ToUpper())
                //{
                //    if (iEmployee == 0 && HttpContext.Session.GetString("Role") != "Admin")
                //    {
                //        return Json(new { success = false, message = "Please select an employee", data = "" });
                //    }
                //    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                //        , HttpContext.Session.GetString("UserName")
                //        , "AttendanceReport", "GetAttendanceReportData", "Employeewise attendance Report generated");

                //    var response = getEmployeewiseReport(iEmployee, iCategory, EmployeeName, CategoryName, iMonth, iYear, ReportType);
                //    return Json(new { success = true, message = "", data = response });
                //}
                //else
                //{
                //    if (iEmployee == 0 && HttpContext.Session.GetString("Role") != "Admin")
                //    {
                //        return Json(new { success = false, message = "Please select an employee", data = "" });
                //    }
                //    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                //        , HttpContext.Session.GetString("UserName")
                //        , "AttendanceReport", "GetAttendanceReportData", "Categorywise attendance Report generated");

                //    var response = getCategorywiseReport(iEmployee, iCategory, EmployeeName, CategoryName, iMonth, iYear, ReportType);
                //    return Json(new { success = true, message = "", data = response });
                //}

                if (iEmployee == 0 && HttpContext.Session.GetString("Role") != "Admin")
                {
                    return Json(new { success = false, message = "Please select an employee", data = "" });
                }
                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "AttendanceReport", "GetAttendanceReportData", "Employeewise attendance Report generated");

                var response = getEmployeewiseReport(iEmployee, iBranch, EmployeeName, BranchName, iMonth, iYear, ReportType);
                return Json(new { success = true, message = "", data = response });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = "", days = 0 });
            }

            //make above query distinct
        }
        private List<Dictionary<string, string>> getCategorywiseReport(int iEmployee, int iCategory,
           string EmployeeName, string CategoryName, int iMonth, int iYear, string ReportType)
        {

            var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();
            //entity frameowrk to call store procedure
            //List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
            //    ($@"exec sp_AttendanceReport {iEmployee}, {iCategory}, {iMonth}, {iYear},{ReportType}").ToList();

            int daysInMonth = DateTime.DaysInMonth(iYear, iMonth);

            DateTime fromDate = new DateTime(iYear, iMonth, 1);
            DateTime toDate = new DateTime(iYear, iMonth, daysInMonth);
            if (startMonth == 1)
            {

            }
            else
            {
                // From date: 26th of previous month
                fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                // To date: 25th of current month
                toDate = new DateTime(iYear, iMonth, startMonth - 1);
            }
            //// From date: 26th of previous month
            //DateTime fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
            //// To date: 25th of current month
            //DateTime toDate = new DateTime(iYear, iMonth, startMonth - 1);

            string strFromDate = fromDate.ToString("yyyy-MM-dd");
            string strToDate = toDate.ToString("yyyy-MM-dd");

            List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                    ($@"exec sp_AttendanceReportMothStartDay {iEmployee}, {iCategory}, {fromDate}, {toDate},{ReportType}").ToList();

            List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>(); //list of dictionary to store attendance data

            //get distinct employee from attendanceReportModels and return name and masterid
            //List<AttendanceReportModel> distinctEmployees = attendanceReportModels.GroupBy(x => x.sEmployeeName).Select(x => x.First()).ToList();
            List<AttendanceReportModel> distinctEmployees = attendanceReportModels.GroupBy(x => x.eMasterid).Select(x => x.First()).ToList();


            for (int iIteration = 0; iIteration < distinctEmployees.Count; iIteration++)
            {

                //// From date: 26th of previous month
                //fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                //// To date: 25th of current month
                //toDate = new DateTime(iYear, iMonth, startMonth - 1);

                if (startMonth == 1)
                {
                    fromDate = new DateTime(iYear, iMonth, 1);
                    toDate = new DateTime(iYear, iMonth, daysInMonth);
                }
                else
                {
                    // From date: 26th of previous month
                    fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                    // To date: 25th of current month
                    toDate = new DateTime(iYear, iMonth, startMonth - 1);
                }


                Dictionary<string, string> attendancedata = new Dictionary<string, string>();
                attendancedata.Add("EmployeeName", distinctEmployees[iIteration].sEmployeeName);
                //attendancedata.Add("CategoryName", CategoryName);
                attendancedata.Add("EmployeeId", distinctEmployees[iIteration].sEmployeeCode);

                //for (int i = 0; i < daysInMonth; i++)
                while (fromDate <= toDate)
                {
                    string sHoliday = "";
                    //var date = new DateTime(iYear, iMonth, i + 1);
                    var date = new DateTime(iYear, fromDate.Month, fromDate.Day);
                    //check if date is sunday
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        sHoliday = "Sunday";
                    }
                    AttendanceReportModel? attendance = attendanceReportModels.Where
                        (x => x.LogDate == date.ToString("dd-MM-yyyy")
                        && x.eMasterid == distinctEmployees[iIteration].eMasterid).FirstOrDefault();
                    if (attendance == null)
                    {
                        attendancedata.Add("day-" + fromDate.Day + "-in", "");
                        attendancedata.Add("day-" + fromDate.Day + "-out", "");
                        attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                        attendancedata.Add("remarks-" + fromDate.Day, "");
                    }
                    else
                    {
                        if (attendance.Status.Contains("Leave"))
                        {
                            attendancedata.Add("day-" + fromDate.Day + "-in", "");
                            attendancedata.Add("day-" + fromDate.Day + "-out", "");
                            attendancedata.Add("day-" + fromDate.Day + "-ph", "");
                            attendancedata.Add("remarks-" + fromDate.Day, attendance.Status);
                        }
                        else
                        {
                            var result = _db.Database.SqlQuery<ShiftTimeResult>($@"
                                select cast(cast(SinTime as time) as datetime) sintime ,
                                cast(cast(SoutTime as time) as datetime) SoutTime 
                                from (
                                SELECT 
                                DATEADD(SECOND, 59,DATEADD(minute, s.iAllowlateminute, s.sStartTime)) AS SinTime,
                                DATEADD(minute, -s.iAllowearlyminute, s.sEndTime) AS SoutTime
                                FROM tblShiftDefinition s
                                JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift
                                WHERE sa.iDay = {fromDate.Day + 1} AND sa.iMonth = {iMonth}
                                AND sa.iEmployee = {distinctEmployees[iIteration].eMasterid} AND sa.iYear = {iYear}
                                )t
                                ").ToList();

                            if (result.Count > 0)
                            {
                                var shiftTime = result[0];


                                if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay
                                    && attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                {
                                    attendancedata.Add("remarks-" + fromDate.Day, "Late In/Early Out");
                                }
                                else if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay)
                                {
                                    attendancedata.Add("remarks-" + fromDate.Day, "Late In");
                                }
                                else if (attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                {
                                    attendancedata.Add("remarks-" + fromDate.Day, "Early Out");
                                }
                                else
                                {
                                    attendancedata.Add("remarks-" + fromDate.Day, "");
                                }
                            }
                            else
                            {
                                attendancedata.Add("remarks-" + fromDate.Day, "");
                            }

                            attendancedata.Add("day-" + fromDate.Day + "-in", attendance.InTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-out", attendance.OutTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                        }
                    }
                    fromDate = fromDate.AddDays(1); //increment the date by 1 day
                }
                attendanceData.Add(attendancedata);
            }
            return (attendanceData);

        }


        private List<Dictionary<string, string>> getEmployeewiseReport(int iEmployee, int iBranch,
           string EmployeeName, string BranchName, int iMonth, int iYear, string ReportType)
        {
            try
            {
                database = HttpContext.Session.GetString("Database");
                string strBranchList = HttpContext.Session.GetString("BranchList");

                List<Employee>? sEmployeeNameList;
                if (ReportType == "EmployeeWise")
                    sEmployeeNameList = _db.Employees.Where(x => x.iMasterid == iEmployee || iEmployee == 0 && x.bEmployeeResign == false).ToList();
                else
                    sEmployeeNameList = _db.Employees.Where(a => a.bEmployeeResign == false).ToList();

                if (!string.IsNullOrEmpty(strBranchList))
                    sEmployeeNameList = sEmployeeNameList.Where(x => strBranchList.Split(',').Contains(x.iBranch.ToString())).ToList();

                if (iBranch != 0 && ReportType != "EmployeeWise")
                    sEmployeeNameList = sEmployeeNameList.Where(x => x.iBranch == iBranch).ToList();

                List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>(); //list of dictionary to store attendance data
                foreach (var semp in sEmployeeNameList)
                {

                    var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();


                    //Employee? sEmployeeName = _db.Employees.Where(x => x.iMasterid == semp.iMasterid).FirstOrDefault();

                    Employee? sEmployeeName = (from e in _db.Employees
                                               join b in _db.Branches
                                               on e.iBranch equals b.iMasterid into empBranch
                                               from branch in empBranch.DefaultIfEmpty()   // left join
                                               where e.iMasterid == semp.iMasterid
                                               select new Employee
                                               {
                                                   sEmployeeName = e.sEmployeeName,
                                                   sEmployeeCode = e.sEmployeeCode,
                                                   iMasterid = e.iMasterid,
                                                   BranchName = branch.sName ?? ""
                                               }).FirstOrDefault();

                    int daysInMonth = DateTime.DaysInMonth(iYear, iMonth);

                    DateTime fromDate = new DateTime(iYear, iMonth, 1);

                    DateTime toDate = new DateTime(iYear, iMonth, daysInMonth);
                    if (startMonth == 1)
                    {

                    }
                    else
                    {
                        // From date: 26th of previous month
                        fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                        // To date: 25th of current month
                        toDate = new DateTime(iYear, iMonth, startMonth - 1);
                    }


                    string strFromDate = fromDate.ToString("yyyy-MM-dd");
                    string strToDate = toDate.ToString("yyyy-MM-dd");

                    //List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                    //    ($@"exec sp_AttendanceReportMothStartDay {semp.iMasterid}, {iBranch}, {strFromDate}, {strToDate},{ReportType}").ToList();

                    Dictionary<string, string> attendancedata = new Dictionary<string, string>();

                    attendancedata.Add("EmployeeName", sEmployeeName.sEmployeeName);
                    attendancedata.Add("EmployeeId", sEmployeeName.sEmployeeCode);
                    attendancedata.Add("BranchName", sEmployeeName.BranchName ?? "");

                    //month start day will be 26th of previous month and 25th of current month loop
                    //for (int i = 0; i < daysInMonth; i++)
                    while (fromDate <= toDate)
                    {
                        List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                            ($@"exec sp_DailyAttendanceReport {fromDate.Day} ,{fromDate.Month}, {fromDate.Year},{semp.iMasterid}").ToList();

                        string sHoliday = "";
                        var date = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);

                        if (date.Day == 29)
                        {

                        }
                        //check if date is sunday
                        if (date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            sHoliday = "Sunday";
                        }
                        AttendanceReportModel? attendance = attendanceReportModels.Where(x => x.LogDate == date.ToString("dd-MM-yyyy")).FirstOrDefault();

                        if (attendance == null)
                        {
                            var shiftWoff = _db.shiftAllocations
                                .Where(sa => sa.iEmployee == semp.iMasterid && sa.dDate == fromDate)
                                .Select(sa => sa.iShift)
                                .FirstOrDefault();
                            if (shiftWoff == 0)
                            {
                                attendancedata.Add("day-" + fromDate.Day + "-in", "");
                                attendancedata.Add("day-" + fromDate.Day + "-out", "");
                                attendancedata.Add("day-" + fromDate.Day + "-ph", "Weekly Off");
                                attendancedata.Add("remarks-" + fromDate.Day, "");
                            }
                            else
                            {
                                var shift = (from sa in _db.shiftAllocations
                                    join sd in _db.Shifts
                                        on sa.iShift equals sd.iMasterid
                                    where sa.iEmployee == 1 && sa.dDate == fromDate
                                    let startDate = DateTime.Parse(
                                    DateTime.Today.ToString("yyyy-MM-dd") + " "
                                    + sd.sStartTime.ToString().Substring(0, 8))
                                    let endDate = startDate.AddHours(sd.fWorkingHour)
                                    select new
                                    {
                                        ShiftType = (endDate.Date == startDate.Date)
                                                ? "Day"
                                                : "Night"
                                    }).FirstOrDefault();
                                DateTime dDate = fromDate;
                                if (shift.ShiftType == "Night")
                                    dDate = dDate.AddDays(-1);

                                var result = (from leave in _db.leaveApplications
                                    join e in _db.Employees
                                        on leave.iEmployee equals e.iMasterid
                                    join ld in _db.Leaves
                                        on leave.iLeaveType equals ld.iMasterid into ldJoin
                                    from ld in ldJoin.DefaultIfEmpty() // left join
                                    where leave.iEmployee == sEmployeeName.iMasterid
                                        && leave.dFromDate.Date == new DateTime(dDate.Year, dDate.Month, dDate.Day)
                                    select new
                                    {
                                        Status = ld.sLeaveCode == "CL" ? "LeaveCL"
                                                : ld.sLeaveCode == "EL" ? "LeaveEL"
                                                : "Leave"
                                    }).ToList();

                                if (result.ToString().Contains("Leave"))
                                {
                                    var preferences = _db.Preferences.FirstOrDefault();
                                    int iSecondLevelAppprovalAuthority = 0;
                                    if (preferences != null)
                                    {
                                        iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                                    }

                                    if (iSecondLevelAppprovalAuthority > 0)
                                    {
                                        int leaveStatus = 0;
                                        if (attendance.Status == "")
                                            leaveStatus = _db.Database.SqlQueryRaw<int>($@"select id as value from tblLeaveApplication 
                                        where iEmployee={attendance.eMasterid} and dFromDate='{dDate.ToString("yyyy-MM-dd")}'
                                         and iApproved2=1").FirstOrDefault();
                                        if (leaveStatus <= 0)
                                        {
                                            attendance.Status = "";
                                        }
                                    }


                                    attendancedata.Add("day-" + fromDate.Day + "-in", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-out", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-ph", "");
                                    attendancedata.Add("remarks-" + fromDate.Day, attendance.Status);
                                }
                                else
                                {
                                    attendancedata.Add("day-" + fromDate.Day + "-in", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-out", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                                    attendancedata.Add("remarks-" + fromDate.Day, "");
                                }
                            }
                        }
                        else
                        {
                            attendancedata.Add("remarks-" + fromDate.Day, attendance.Status);

                            attendancedata.Add("day-" + fromDate.Day + "-in", attendance.InTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-out", attendance.OutTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                        }

                        fromDate = fromDate.AddDays(1); //increment the date by 1 day
                    }
                    attendanceData.Add(attendancedata);
                }
                return (attendanceData);

            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, string>>(); //return empty list in case of error
            }
        }


        private List<Dictionary<string, string>> getEmployeewiseReportOld(int iEmployee, int iCategory,
            string EmployeeName, string BranchName, int iMonth, int iYear, string ReportType)
        {
            try
            {

                List<Employee>? sEmployeeNameList = _db.Employees.Where(x => x.iMasterid == iEmployee || iEmployee == 0).ToList();
                List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>(); //list of dictionary to store attendance data
                foreach (var semp in sEmployeeNameList)
                {

                    var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();

                    //get EmployeeName from Masterid from employee table
                    //Employee? sEmployeeName = _db.Employees.Where(x => x.iMasterid == iEmployee).FirstOrDefault();
                    Employee? sEmployeeName = _db.Employees.Where(x => x.iMasterid == semp.iMasterid).FirstOrDefault();

                    int daysInMonth = DateTime.DaysInMonth(iYear, iMonth);

                    DateTime fromDate = new DateTime(iYear, iMonth, 1);

                    DateTime toDate = new DateTime(iYear, iMonth, daysInMonth);
                    if (startMonth == 1)
                    {

                    }
                    else
                    {
                        // From date: 26th of previous month
                        fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                        // To date: 25th of current month
                        toDate = new DateTime(iYear, iMonth, startMonth - 1);
                    }
                    //entity frameowrk to call store procedure

                    //List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                    //    ($@"exec sp_AttendanceReportMothStartDay {iEmployee}, {iCategory}, {fromDate}, {toDate},{ReportType}").ToList();

                    string strFromDate = fromDate.ToString("yyyy-MM-dd");
                    string strToDate = toDate.ToString("yyyy-MM-dd");

                    List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                        ($@"exec sp_AttendanceReportMothStartDay {semp.iMasterid}, {iCategory}, {strFromDate}, {strToDate},{ReportType}").ToList();

                    Dictionary<string, string> attendancedata = new Dictionary<string, string>();

                    attendancedata.Add("EmployeeName", sEmployeeName.sEmployeeName);
                    //attendancedata.Add("CategoryName", attendanceReportModels[0].sCategory);
                    attendancedata.Add("EmployeeId", sEmployeeName.sEmployeeCode);


                    //month start day will be 26th of previous month and 25th of current month loop
                    //for (int i = 0; i < daysInMonth; i++)
                    while (fromDate <= toDate)
                    {
                        string sHoliday = "";
                        var date = new DateTime(iYear, fromDate.Month, fromDate.Day);

                        if (date.Day == 29)
                        {

                        }
                        //check if date is sunday
                        if (date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            sHoliday = "Sunday";
                        }
                        AttendanceReportModel? attendance = attendanceReportModels.Where(x => x.LogDate == date.ToString("dd-MM-yyyy")).FirstOrDefault();

                        if (attendance == null)
                        {
                            attendancedata.Add("day-" + fromDate.Day + "-in", "");
                            attendancedata.Add("day-" + fromDate.Day + "-out", "");
                            attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                            attendancedata.Add("remarks-" + fromDate.Day, "");
                        }
                        else
                        {
                            if (attendance.Status.Contains("Leave"))
                            {
                                attendancedata.Add("day-" + fromDate.Day + "-in", "");
                                attendancedata.Add("day-" + fromDate.Day + "-out", "");
                                attendancedata.Add("day-" + fromDate.Day + "-ph", "");
                                attendancedata.Add("remarks-" + fromDate.Day, attendance.Status);
                            }
                            else
                            {
                                var result = _db.Database.SqlQuery<ShiftTimeResult>($@"
                                select cast(cast(SinTime as time) as datetime) sintime ,
                                cast(cast(SoutTime as time) as datetime) SoutTime 
                                from (
                                SELECT 
                                DATEADD(SECOND, 59,DATEADD(minute, s.iAllowlateminute, s.sStartTime)) AS SinTime,
                                DATEADD(minute, -s.iAllowearlyminute, s.sEndTime) AS SoutTime
                                FROM tblShiftDefinition s
                                JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift
                                WHERE sa.iDay = {fromDate.Day + 1} AND sa.iMonth = {iMonth}
                                AND sa.iEmployee = {semp.iMasterid} AND sa.iYear = {iYear}
                                )t
                                ").ToList();

                                if (result.Count > 0)
                                {
                                    var shiftTime = result[0];

                                    if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay
                                        && attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                    {
                                        attendancedata.Add("remarks-" + fromDate.Day, "Late In/Early Out");
                                    }
                                    else if (attendance.InTime.TimeOfDay > shiftTime.SinTime.TimeOfDay)
                                    {
                                        attendancedata.Add("remarks-" + fromDate.Day, "Late In");
                                    }
                                    else if (attendance.OutTime.TimeOfDay < shiftTime.SoutTime.TimeOfDay)
                                    {
                                        attendancedata.Add("remarks-" + fromDate.Day, "Early Out");
                                    }
                                    else
                                    {
                                        attendancedata.Add("remarks-" + fromDate.Day, "");
                                    }
                                }
                                else
                                {
                                    attendancedata.Add("remarks-" + fromDate.Day, "");
                                }

                                attendancedata.Add("day-" + fromDate.Day + "-in", attendance.InTime.ToString("HH:mm"));
                                attendancedata.Add("day-" + fromDate.Day + "-out", attendance.OutTime.ToString("HH:mm"));
                                attendancedata.Add("day-" + fromDate.Day + "-ph", sHoliday);
                            }
                        }

                        fromDate = fromDate.AddDays(1); //increment the date by 1 day
                    }
                    attendanceData.Add(attendancedata);
                }
                return (attendanceData);

            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, string>>(); //return empty list in case of error
            }
        }

        //funciton ExportToExcel
        [HttpPost]
        //public async Task<IActionResult> ExportToExcel(List<Dictionary<string, string>> attendanceData, string month, string year)
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
        {
            try
            {
                List<Dictionary<string, string>> attendanceData = request.AttendanceData;
                //create a new workbook
                //write to excel file
                using (XLWorkbook workbook = new XLWorkbook())
                {

                    //create a new worksheet
                    IXLWorksheet worksheet = workbook.Worksheets.Add("Attendance Report");
                    //add header
                    //make worksheet visible
                    worksheet.Visibility = XLWorksheetVisibility.Visible;

                    // 🔹 Add Company Name (Row 1)
                    string companyName = HttpContext.Session.GetString("CompanyName");
                    worksheet.Cell(1, 1).Value = companyName;  // replace with your variable
                    worksheet.Range(1, 1, 1, attendanceData.Count + 3).Merge(); // Merge across all columns
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 🔹 Add Report Title (Row 2)
                    worksheet.Cell(2, 1).Value = "Attendance Report: " + request.Month + "-" + request.Year; // replace with your title
                    worksheet.Range(2, 1, 2, attendanceData.Count + 3).Merge();
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Cell(2, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                    worksheet.Cell(3, 1).Value = "Date";
                    for (int i = 1; i < (attendanceData[0].Count) / 3; i++)
                    {
                        worksheet.Cell(i + 3, 1).Value = attendanceData[0]["day-" + i + "date"];
                    }
                    int iColumn = 2;
                    int iRow = 4;
                    for (int c = 0; c < attendanceData.Count; c++)
                    {
                        string strValue = attendanceData[c]["EmployeeName"] + "\n" + attendanceData[c]["EmployeeId"] + "\n" + attendanceData[c]["BranchName"];
                        worksheet.Cell(3, c + 2).Value = strValue;

                        for (int r = 0; r < (attendanceData[0].Count - 2) / 3; r++)
                        {
                            worksheet.Cell(iRow, c + 2).Value = attendanceData[c]["day-" + (r + 1)];

                            if (attendanceData[c]["day-" + (r + 1) + "bg"] != null)
                            {
                                //rgb(241, 158, 158)
                                //get the rgb value from the string
                                string bgColor = "";
                                if (attendanceData[c]["day-" + (r + 1) + "bg"].ToString() != "")
                                    bgColor = attendanceData[c]["day-" + (r + 1) + "bg"].ToString().Substring(4, attendanceData[c]["day-" + (r + 1) + "bg"].ToString().IndexOf(")") - 4);
                                if (bgColor == "21, 87, 36")
                                {
                                    bgColor = "144, 238, 144";
                                }
                                //convert commaseparated string into list
                                string[] rgb = bgColor.Split(',');

                                int rr = Convert.ToInt32(rgb[0]);
                                int gg = Convert.ToInt32(rgb[1]);
                                int bb = Convert.ToInt32(rgb[2]);
                                worksheet.Cell(iRow, c + 2).Style.Fill.BackgroundColor = XLColor.FromArgb(rr, gg, bb);
                            }
                            iRow++;
                        }
                    }
                    worksheet.Range(3, 1, 3, attendanceData.Count + 1).Style.Font.Bold = true;
                    worksheet.Range(3, 1, 3, attendanceData.Count + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFFFE1");
                    worksheet.Columns().AdjustToContents();
                    worksheet.Cell("B3").Style.Alignment.WrapText = true;
                    //foreach (var item in attendanceData)
                    //{
                    //    worksheet.Cell(iRow, 1).Value = item["EmployeeName"];
                    //    worksheet.Cell(iRow, 2).Value = item["CategoryName"];
                    //    int jColumn = 3;
                    //    for (int j = 0; j < (attendanceData[0].Count - 2) / 3; j++)
                    //    {
                    //        worksheet.Cell(iRow, jColumn).Value = item["day-" + j];
                    //        if (item["day-" + j + "bg"] != null)
                    //        {
                    //            //rgb(241, 158, 158)
                    //            //get the rgb value from the string
                    //            string bgColor = item["day-" + j + "bg"].ToString().Substring(4, item["day-" + j + "bg"].ToString().IndexOf(")") - 4);
                    //            if (bgColor == "21, 87, 36")
                    //            {
                    //                bgColor = "144, 238, 144";
                    //            }
                    //            //convert commaseparated string into list
                    //            string[] rgb = bgColor.Split(',');

                    //            int r = Convert.ToInt32(rgb[0]);
                    //            int g = Convert.ToInt32(rgb[1]);
                    //            int b = Convert.ToInt32(rgb[2]);
                    //            worksheet.Cell(iRow, jColumn).Style.Fill.BackgroundColor = XLColor.FromArgb(r, g, b);
                    //        }
                    //        jColumn++;

                    //    }
                    //    iRow++;
                    //}
                    //save the workbook to a file
                    string fileName = $"AttendanceReport_{request.Month}_{request.Year}.xlsx";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "AttendanceReport", "GetAttendanceReportData", "Excel generated, filename: " + fileName);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = "" });
            }
        }
    }
}
