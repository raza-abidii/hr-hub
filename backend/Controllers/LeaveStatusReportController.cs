using EMSSolution.DataAccess;
using EMSSolution.DatabaseAccessLayer;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EMSSolution.Controllers
{
    public class LeaveStatusReportController : Controller
    {
        private readonly ApplicationDBContext _db;
        private IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public LeaveStatusReportController(ApplicationDBContext db,IUserActivityLogger userActivityLogger)
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

        [HttpPost]
        public IActionResult GetLeaveStatusReport(int empId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                DataLayer dl = new DataLayer();
                if (HttpContext.Session.GetString("Role") != "Admin" && empId==0)
                {
                    return Json(new { success = false, message = "Select Employee", data = "" });
                   
                }
                else
                {
                    DateTime fromdt = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                    // To date: 25th of current month
                    DateTime todt = new DateTime(toDate.Year,toDate.Month, toDate.Day);

                    // This action can be used to render the view for manual leave approval.
                    List<LeaveStatusReport> leaveStatusReports = _db.Database.SqlQuery<LeaveStatusReport>
                    ($@"exec sp_getEmployeeLeaveStatus {empId},{fromdt},{todt}").ToList();

                    var preferences = _db.Preferences.FirstOrDefault();
                    int iSecondLevelAppprovalAuthority = 0;
                    if (preferences != null)
                    {
                        iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                    }
                    string strQry = "",strErrMess="";
                    if (iSecondLevelAppprovalAuthority > 0)
                    {
                        foreach (var lsr in leaveStatusReports)
                        {
                            strQry = $@"select case when iApproved2=0 then 'Pending' 
                                when iApproved2=-1 then 'Rejected' else 'Approved' 
                                end finalstatus
                                from tblLeaveApplication where id={lsr.Leaveid} 
                                and iApprovedAuthority2<>0  ";
                            DataSet ds = dl.GetData(strQry, ref strErrMess);
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
                    return Json(new { success = true, message = "", data = leaveStatusReports });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in GetLeaveApplication: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while fetching leave applications." });
            }
        }
    }
}
