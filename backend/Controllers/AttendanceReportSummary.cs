using DocumentFormat.OpenXml.Office.Y2022.FeaturePropertyBag;
using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EMSSolution.Controllers
{
    public class AttendanceReportSummary : Controller
    {
        private readonly IUserActivityLogger _userActivityLogger;
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        public AttendanceReportSummary(IUserActivityLogger userActivityLogger, ApplicationDBContext db)
        {
            _userActivityLogger = userActivityLogger;
            _db = db;
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
                if (ReportType.ToUpper() == "EmployeeWise".ToUpper())
                {
                    

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "AttendanceReport", "GetAttendanceReportData", "Employeewise attendance Report generated");

                    var response = getEmployeewiseReport(iEmployee, iBranch, EmployeeName, BranchName, iMonth, iYear, ReportType);
                    return Json(new { success = true, message = "", data = response });
                }
                else
                {
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "AttendanceReport", "GetAttendanceReportData", "Categorywise attendance Report generated");

                    var response = getCategorywiseReport(iEmployee, iBranch, EmployeeName, BranchName, iMonth, iYear, ReportType);
                    return Json(new { success = true, message = "", data = response });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = "", days = 0 });
            }

            //make above query distinct
        }
        private List<Dictionary<string, string>> getCategorywiseReport(int iEmployee, int iBranch,
           string EmployeeName, string BranchName, int iMonth, int iYear, string ReportType)
        {
            var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();
            //entity frameowrk to call store procedure
            
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

            List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                    ($@"exec sp_AttendanceReportMothStartDay {iEmployee}, {iBranch}, {fromDate}, {toDate},{ReportType}").ToList();

            List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>(); //list of dictionary to store attendance data

            //get distinct employee from attendanceReportModels and return name and masterid
            List<AttendanceReportModel> distinctEmployees = attendanceReportModels.GroupBy(x => x.eMasterid).Select(x => x.First()).ToList();
            for (int iIteration = 0; iIteration < distinctEmployees.Count; iIteration++)
            {
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
                attendancedata.Add("EmployeeCode", distinctEmployees[iIteration].sEmployeeCode);
                attendancedata.Add("BranchName", BranchName);

                //attendancedata.Add("DaysInMonth", daysInMonth.ToString());
                attendancedata.Add("DaysInMonth", ((toDate - fromDate).Days + 1).ToString());
                int iHoliday = 0, iLeave = 0, iPresent = 0, iAbsent = 0;
                while (fromDate <= toDate)
                {
                    string sHoliday = "";
                    var date = new DateTime(iYear, fromDate.Month, fromDate.Day);
                    //check if date is sunday
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        sHoliday = "Sunday";
                        iHoliday++;
                    }
                    AttendanceReportModel? attendance = attendanceReportModels.Where
                        (x => x.LogDate == date.ToString("dd-MM-yyyy")
                        && x.eMasterid == distinctEmployees[iIteration].eMasterid).FirstOrDefault();
                    if (attendance == null)
                    {
                        if (sHoliday != "Sunday")
                            iAbsent++;
                    }
                    else
                    {
                        if (attendance.Status.Contains("Leave"))
                        {
                            iLeave++;
                        }
                        else
                        {
                            iPresent++;
                        }
                    }
                    fromDate = fromDate.AddDays(1); //increment the date by 1 day
                }
                attendancedata.Add("Leave", iLeave.ToString());
                attendancedata.Add("Holiday", iHoliday.ToString());
                attendancedata.Add("Present", iPresent.ToString());
                attendancedata.Add("Absent", iAbsent.ToString());
                attendancedata.Add("PaidDays", Convert.ToString(iPresent + iHoliday + iLeave));
                attendanceData.Add(attendancedata);
            }
            return (attendanceData);
        }
        private List<Dictionary<string, string>> getEmployeewiseReport(int iEmployee, int iBranch,
            string EmployeeName, string BranchName, int iMonth, int iYear, string ReportType)
        {
            try
            {
                var startMonth = _db.companies.Select(x => x.MonthStartfrom).FirstOrDefault();
                //get EmployeeName from Masterid from employee table
                Employee? sEmployeeName = _db.Employees.Where(x => x.iMasterid == iEmployee ).FirstOrDefault();
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

                List<AttendanceReportModel> attendanceReportModels = _db.Database.SqlQuery<AttendanceReportModel>
                    ($@"exec sp_AttendanceReportMothStartDay {iEmployee}, {iBranch}, {fromDate}, {toDate},{ReportType}").ToList();

                //list of dictionary to store attendance data
                List<Dictionary<string, string>> attendanceData = new List<Dictionary<string, string>>();
                Dictionary<string, string> attendancedata = new Dictionary<string, string>();

                attendancedata.Add("EmployeeName", sEmployeeName.sEmployeeName);
                attendancedata.Add("EmployeeCode", attendanceReportModels[0].sEmployeeCode);
                attendancedata.Add("BranchName", attendanceReportModels[0].sBranch);

                attendancedata.Add("DaysInMonth", ((toDate-fromDate).Days+1).ToString());
                int iHoliday = 0, iLeave = 0, iPresent = 0, iAbsent = 0;
                //for (int i = 0; i < daysInMonth; i++)
                while (fromDate <= toDate)
                {
                    string sHoliday = "";
                    var date = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                    //check if date is sunday
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        sHoliday = "Sunday";
                        iHoliday++;
                    }
                    AttendanceReportModel? attendance = attendanceReportModels.Where(x => x.LogDate == date.ToString("dd-MM-yyyy")).FirstOrDefault();

                    if (attendance == null)
                    {
                        if (sHoliday != "Sunday")
                            iAbsent++;
                    }
                    else
                    {
                        if (attendance.Status.Contains("Leave"))
                        {
                            iLeave++;
                        }
                        else
                        {
                            iPresent++;
                        }
                    }
                    fromDate = fromDate.AddDays(1); //increment the date by 1 day
                }
                attendancedata.Add("Leave", iLeave.ToString());
                attendancedata.Add("Holiday", iHoliday.ToString());
                attendancedata.Add("Present", iPresent.ToString());
                attendancedata.Add("Absent", iAbsent.ToString());
                attendancedata.Add("PaidDays",Convert.ToString( iPresent+iHoliday+iLeave));
                attendanceData.Add(attendancedata);
                return (attendanceData);
            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, string>>(); //return empty list in case of error
            }
        }
    }
}
