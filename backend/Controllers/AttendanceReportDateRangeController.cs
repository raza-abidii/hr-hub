using ClosedXML.Excel;
using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EMSSolution.Controllers
{
    public class AttendanceReportDateRangeController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public IActionResult Index()
        {
            return View();
        }
        public AttendanceReportDateRangeController(ApplicationDBContext db, IUserActivityLogger? userActivityLogger)
        {
            _db = db;
            _userActivityLogger = userActivityLogger;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        public async Task<IActionResult> GetAttendanceReportData(int iEmployee, int iBranch,
          string EmployeeName, string BranchName, DateTime FromDate, DateTime ToDate, string ReportType)
        {
            try
            {
                if (iEmployee == 0 && HttpContext.Session.GetString("Role") != "Admin")
                {
                    return Json(new { success = false, message = "Please select an employee", data = "" });
                }
                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "AttendanceReport", "GetAttendanceReportData", "Employeewise attendance Report generated");

                var response = getReport(iEmployee, iBranch, EmployeeName, BranchName, FromDate, ToDate, ReportType);
                return Json(new { success = true, message = "", data = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = "", days = 0 });
            }

        }

        private List<Dictionary<string, string>> getCategorywiseReport(int iEmployee, int iCategory,
          string EmployeeName, string CategoryName, DateTime toDate, DateTime fromDate, string ReportType)
        {

            var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();
            //entity frameowrk to call store procedure
            //List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
            //    ($@"exec sp_AttendanceReport {iEmployee}, {iCategory}, {iMonth}, {iYear},{ReportType}").ToList();

            //int daysInMonth = DateTime.DaysInMonth(iYear, iMonth);

            //DateTime fromDate = new DateTime(iYear, iMonth, 1);
            //DateTime toDate = new DateTime(iYear, iMonth, daysInMonth);
            if (startMonth == 1)
            {

            }
            else
            {
                //// From date: 26th of previous month
                //fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                //// To date: 25th of current month
                //toDate = new DateTime(iYear, iMonth, startMonth - 1);
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

                //if (startMonth == 1)
                //{
                //    fromDate = new DateTime(iYear, iMonth, 1);
                //    toDate = new DateTime(iYear, iMonth, daysInMonth);
                //}
                //else
                //{
                //    // From date: 26th of previous month
                //    fromDate = new DateTime(iYear, iMonth, 1).AddMonths(-1).AddDays(startMonth - 1);
                //    // To date: 25th of current month
                //    toDate = new DateTime(iYear, iMonth, startMonth - 1);
                //}


                Dictionary<string, string> attendancedata = new Dictionary<string, string>();
                attendancedata.Add("EmployeeName", distinctEmployees[iIteration].sEmployeeName);
                //attendancedata.Add("CategoryName", CategoryName);
                attendancedata.Add("EmployeeId", distinctEmployees[iIteration].sEmployeeCode);

                //for (int i = 0; i < daysInMonth; i++)
                while (fromDate <= toDate)
                {
                    string sHoliday = "";
                    //var date = new DateTime(iYear, iMonth, i + 1);
                    var date = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
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
                            var preferences = _db.Preferences.FirstOrDefault();
                            int iSecondLevelAppprovalAuthority = 0;
                            if (preferences != null)
                            {
                                iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                            }

                            if (iSecondLevelAppprovalAuthority > 0)
                            {

                                int leaveStatus = _db.Database.SqlQueryRaw<int>($@"select id as value from tblLeaveApplication 
                                        where iEmployee={attendance.eMasterid} and dFromDate='{fromDate.ToString("yyyy-MM-dd")}'
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
                            var result = _db.Database.SqlQuery<ShiftTimeResult>($@"
                                select cast(cast(SinTime as time) as datetime) sintime ,
                                cast(cast(SoutTime as time) as datetime) SoutTime 
                                from (
                                SELECT 
                                DATEADD(SECOND, 59,DATEADD(minute, s.iAllowlateminute, s.sStartTime)) AS SinTime,
                                DATEADD(minute, -s.iAllowearlyminute, s.sEndTime) AS SoutTime
                                FROM tblShiftDefinition s
                                JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift
                                WHERE sa.iDay = {fromDate.Day + 1} AND sa.iMonth = {fromDate.Month}
                                AND sa.iEmployee = {distinctEmployees[iIteration].eMasterid} AND sa.iYear = {fromDate.Year}
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
        private List<Dictionary<string, string>> getReport(int iEmployee, int iBranch,
            string EmployeeName, string BranchName, DateTime fromDate, DateTime toDate, string ReportType)
        {
            try
            {
                DateTime fDate = fromDate;
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

                    fromDate = fDate;
                    string strFromDate = fromDate.ToString("yyyy-MM-dd");
                    string strToDate = toDate.ToString("yyyy-MM-dd");
                    Dictionary<string, string> attendancedata = new Dictionary<string, string>();
                    attendancedata.Add("EmployeeName", sEmployeeName.sEmployeeName);
                    attendancedata.Add("EmployeeId", sEmployeeName.sEmployeeCode);
                    attendancedata.Add("BranchName", sEmployeeName.BranchName ?? "");

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
                                             where sa.iEmployee == semp.iMasterid && sa.dDate == fromDate
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


                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-in", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-out", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-ph", "");
                                    attendancedata.Add("remarks-" + fromDate.Day + "-" + fromDate.Month, attendance.Status);
                                }
                                else
                                {
                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-in", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-out", "");
                                    attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-ph", sHoliday);
                                    attendancedata.Add("remarks-" + fromDate.Day + "-" + fromDate.Month, "");
                                }
                            }
                        }
                        else
                        {
                            attendancedata.Add("remarks-" + fromDate.Day + "-" + fromDate.Month, attendance.Status);

                            attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-in", attendance.InTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-out", attendance.OutTime.ToString("HH:mm"));
                            attendancedata.Add("day-" + fromDate.Day + "-" + fromDate.Month + "-ph", sHoliday);

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

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequestDateRange request)
        {
            try
            {
                List<Dictionary<string, string>> attendanceData = request.AttendanceData;
                string fromdate = request.fromdate;
                string todate = request.todate;
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
                    worksheet.Range(1, 1, 1, ((attendanceData[0].Count - 2) / 3) + 3).Merge(); // Merge across all columns
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 🔹 Add Report Title (Row 2)
                    worksheet.Cell(2, 1).Value = "Attendance Report: From: " + fromdate + " to " + todate; // replace with your title
                    worksheet.Range(2, 1, 2, ((attendanceData[0].Count - 2) / 3) + 3).Merge();
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Cell(2, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(3, 1).Value = "Employee Name";
                    worksheet.Cell(3, 2).Value = "Employee Id";
                    worksheet.Cell(3, 3).Value = "Branch";
                    int iColumn = 4;

                    //for (int i = 0; i < (attendanceData[0].Count - 2) / 2; i++)
                    for (int i = 1; i <= (attendanceData[0].Count - 2) / 3; i++)
                    {
                        //worksheet.Cell(1, iColumn).Value = "Day " + (i + 1);
                        worksheet.Cell(3, iColumn).Value = attendanceData[0]["day-" + i + "day"];
                        iColumn++;

                    }
                    int iRow = 4;
                    foreach (var item in attendanceData)
                    {
                        worksheet.Cell(iRow, 1).Value = item["EmployeeName"];
                        worksheet.Cell(iRow, 2).Value = item["EmployeeCode"];
                        worksheet.Cell(iRow, 3).Value = item["BranchName"];
                        int jColumn = 4;
                        for (int j = 1; j <= (attendanceData[0].Count - 2) / 3; j++)
                        {
                            worksheet.Cell(iRow, jColumn).Value = item["day-" + j];
                            if (item["day-" + j + "bg"] != null)
                            {
                                //rgb(241, 158, 158)
                                //get the rgb value from the string
                                string bgColor = item["day-" + j + "bg"].ToString().Substring(4, item["day-" + j + "bg"].ToString().IndexOf(")") - 4);
                                if (bgColor == "21, 87, 36")
                                {
                                    bgColor = "144, 238, 144";
                                }
                                //convert commaseparated string into list
                                string[] rgb = bgColor.Split(',');

                                int r = Convert.ToInt32(rgb[0]);
                                int g = Convert.ToInt32(rgb[1]);
                                int b = Convert.ToInt32(rgb[2]);
                                worksheet.Cell(iRow, jColumn).Style.Fill.BackgroundColor = XLColor.FromArgb(r, g, b);
                            }
                            jColumn++;
                            //worksheet.Cell(iRow, jColumn).Value = item["day-" + j + "-out"];
                            //jColumn++;
                            //worksheet.Cell(iRow, jColumn).Value = item["day-" + j + "-ph"];
                            //jColumn++;
                        }
                        iRow++;
                    }
                    //save the workbook to a file
                    string fileName = $"AttendanceReportDateRange.xlsx";
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

