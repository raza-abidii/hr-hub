using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;

namespace EMSSolution.Controllers
{
    public class DashboardController : Controller
    {
        readonly ApplicationDBContext _db;
        string strQry = string.Empty;
        string strErrMsg = string.Empty;
        DataSet ds = new DataSet();
        DatabaseAccessLayer.DataLayer dal = new DatabaseAccessLayer.DataLayer();
        string database = string.Empty;
        public IActionResult Index()
        {
            return View();
        }
        public DashboardController(ApplicationDBContext db)
        {
            _db = db;
           
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }


        public IActionResult DailyLogDashboard(int page = 1, int pageSize = 10, string sortColumn = "LogDate", string sortOrder = "desc")
        {
            try
            {
                
                GenericFunction.WriteLog("DashboardController", "DailyLogDashboard: Hits");
                #region check if Session Live or Expired
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Login", "Account");
                }
                #endregion

                // This action will render the Daily Log Dashboard view
                var empCount = _db.Employees.Count();
                ViewBag.TotalEmployees = empCount;
                GenericFunction.WriteLog("DashboardController", "Employee Count: " + empCount);

                strQry = $@"select isnull(count(deviceId),0) cnt from epush..Devices";
                ds = dal.GetData(strQry, ref strErrMsg);
                ViewBag.TotalDevices = ds.Tables[0].Rows.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["cnt"]) : 0;

                GenericFunction.WriteLog("DashboardController", "Query: " + strQry);

                strQry = $@"select isnull(count(1),0) cnt from epush..DeviceLogs_{DateTime.Now.Month}_{DateTime.Now.Year}";
                ds = dal.GetData(strQry, ref strErrMsg, database);
                if (ds == null)
                    ViewBag.TotalLogsThisMonth = 0;
                else
                    ViewBag.TotalLogsThisMonth = ds.Tables[0].Rows.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["cnt"]) : 0;
                GenericFunction.WriteLog("DashboardController", "Query: " + empCount);

                strQry = $@"select isnull(count(1),0) cnt from epush..DeviceLogs_{DateTime.Now.Month}_{DateTime.Now.Year} 
                    where format(LogDate,'dd/MM/yyyy')=format(getdate(),'dd/MM/yyyy')";
                ds = dal.GetData(strQry, ref strErrMsg, database);
                if (ds == null)
                    ViewBag.TotalLogsToday = 0;
                else
                    ViewBag.TotalLogsToday = ds.Tables[0].Rows.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["cnt"]) : 0;
                GenericFunction.WriteLog("DashboardController", "Query: " + empCount);

                List<LogEntryViewModel> loglist = GetLogData();
                loglist = SortLogs(loglist, sortColumn, sortOrder);

                //var pageSize = 10; // Show 10 records per page
                var totalRecords = loglist.Count;
                var pagedLogs = loglist.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                DailyLogDashboardViewModel dailyLogDashboardViewModel = new DailyLogDashboardViewModel
                {
                    LogEntries = pagedLogs,
                    CurrentPage = page, // Assuming you want to show the first page
                    TotalPages = (int)Math.Ceiling((double)totalRecords / 10), // Assuming 10 entries per page
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder

                };
                return View(dailyLogDashboardViewModel);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                GenericFunction.WriteLog("DashboardController", "DailyLogDashboard: Exception: " + ex.Message);
                return View("Error");
            }
        }

        private List<LogEntryViewModel> SortLogs(List<LogEntryViewModel> logs, string sortColumn, string sortOrder)
        {
            bool ascending = sortOrder.ToLower() == "asc";

            return sortColumn switch
            {
                "DeviceId" => ascending ? logs.OrderBy(x => x.DeviceId).ToList() : logs.OrderByDescending(x => x.DeviceId).ToList(),
                "DeviceName" => ascending ? logs.OrderBy(x => x.DeviceName).ToList() : logs.OrderByDescending(x => x.DeviceName).ToList(),
                "SerialNo" => ascending ? logs.OrderBy(x => x.SerialNo).ToList() : logs.OrderByDescending(x => x.SerialNo).ToList(),
                "EmpId" => ascending ? logs.OrderBy(x => x.EmpId).ToList() : logs.OrderByDescending(x => x.EmpId).ToList(),
                "EmpName" => ascending ? logs.OrderBy(x => x.EmpName).ToList() : logs.OrderByDescending(x => x.EmpName).ToList(),
                "LogDate" => ascending ? logs.OrderBy(x => x.LogDate).ToList() : logs.OrderByDescending(x => x.LogDate).ToList(),
                "VerifyMethod" => ascending ? logs.OrderBy(x => x.VerifyMethod).ToList() : logs.OrderByDescending(x => x.VerifyMethod).ToList(),
                _ => logs.OrderByDescending(x => x.LogDate).ToList() // default
            };
        }
        private List<LogEntryViewModel> GetLogData()
        {
            strQry = $@"select dl.DeviceId,d.DeviceFName DeviceName,d.SerialNumber,
                dl.UserId EmployeeId,isnull(e.sEmployeeName,'') EmployeeName,
                dl.LogDate,vm.VerifyMethodName VerifyMethod
                from epush..DeviceLogs_{DateTime.Now.Month}_{DateTime.Now.Year} dl 
                join epush..VerificationMode vm on vm.VerifyMethodCode=dl.C5
                join epush..Devices d on d.DeviceId=dl.DeviceId
                left join tblEmployee e on e.sEmployeeCode 
                collate SQL_Latin1_General_CP1_CI_AS=dl.UserId collate SQL_Latin1_General_CP1_CI_AS
                where format(LogDate,'dd/MM/yyyy')=format(getdate(),'dd/MM/yyyy')
                order by DeviceLogId desc";
            ds = dal.GetData(strQry, ref strErrMsg, database);
            if (ds==null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return new List<LogEntryViewModel>();
            }

            var logEntries = new List<LogEntryViewModel>();
            ds.Tables[0].AsEnumerable().ToList().ForEach(row =>
            {
                logEntries.Add(new LogEntryViewModel
                {
                    DeviceId = row.Field<Int32>("DeviceId"),
                    DeviceName = row.Field<string>("DeviceName") ?? "",
                    SerialNo = row.Field<string>("SerialNumber") ?? "",
                    EmpId = row.Field<string>("EmployeeId") ?? "",
                    EmpName = row.Field<string>("EmployeeName") ?? "",
                    LogDate = row.Field<DateTime>("LogDate"),
                    VerifyMethod = row.Field<string>("VerifyMethod") ?? ""
                });
            });

            return logEntries;
        }
    }
}
