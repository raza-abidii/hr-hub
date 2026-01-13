using DocumentFormat.OpenXml.Spreadsheet;
using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace EMSSolution.Controllers
{
    public class LeaveApprovalManualController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;

        public LeaveApprovalManualController(ApplicationDBContext db)
        {
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
        [HttpPost]
        public IActionResult GetLeaveApplication(int empId)
        {
            try
            {
                // This action can be used to render the view for manual leave approval.
                var attendanceReportModels = _db.Database.SqlQuery<LeaveApprovalModel>(
                    $@"exec sp_getPendingLeaveDetail {empId}"
                ).ToList();

                var preferences = _db.Preferences.FirstOrDefault();

                int iSecondLevelAppprovalAuthority = 0;
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                }
                string strQry = "";
                if (iSecondLevelAppprovalAuthority != Convert.ToInt32(HttpContext.Session.GetString("UserId")))
                {
                    strQry = $@"select la.id LeaveId, e.sEmployeeName EmployeeName,    
                        e.sEmployeeCode EmployeeCode,la.sRemarks Remarks ,    
                        isnull(repto.sEmployeeName,'') ReportingTo,format(la.dFromDate,'dd-MM-yyyy') dDate    
                        from tblLeaveApplication la    
                        join tblEmployee e on e.iMasterId=la.iEmployee    
                        left join tblEmployee repto on e.iReportingTo=repto.iMasterId    
                        where la.iApproved1=0 ";
                }
                else
                {
                    if (iSecondLevelAppprovalAuthority == Convert.ToInt32(HttpContext.Session.GetString("UserId")))
                    {
                        strQry = $@"select la.id LeaveId, e.sEmployeeName EmployeeName,    
                            e.sEmployeeCode EmployeeCode,la.sRemarks Remarks ,    
                            isnull(repto.sEmployeeName,'') ReportingTo,format(la.dFromDate,'dd-MM-yyyy') dDate    
                            from tblLeaveApplication la    
                            join tblEmployee e on e.iMasterId=la.iEmployee    
                            left join tblEmployee repto on e.iReportingTo=repto.iMasterId    
                            where la.iApproved1>=0 and la.iApproved2=0 and isnull(la.iApprovedAuthority2,0)<>0";
                    }
                }
                var leaveApproval = _db.Database.SqlQuery<LeaveApprovalModel>(
                    FormattableStringFactory.Create(strQry)
                ).ToList();
                return Json(new { success = true, data = leaveApproval });
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in GetLeaveApplication: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Exception in GetLeaveApplication", new { message = "An error occurred while fetching leave applications.:" + ex.Message });

            }
        }
        [HttpPost]
        public IActionResult HandleLeaveAction(int id, string actionType, string remarks)
        {
            try
            {
                string? userName = HttpContext.Session.GetString("UserName");
                string? userId = HttpContext.Session.GetString("UserId");

                var preferences = _db.Preferences.FirstOrDefault();
                int iSecondLevelAppprovalAuthority = 0;
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                }

                if (actionType == "Approve")
                {
                    if (iSecondLevelAppprovalAuthority != Convert.ToInt32(userId))
                    {
                        LeaveApplication? leaveApply = _db.leaveApplications.FirstOrDefault(a => a.id == id);
                        if (leaveApply != null)
                        {
                            leaveApply.iApproved1 = 1;
                            leaveApply.sApprovedBy1 = userName;
                            leaveApply.ApprovalRemarks1 = remarks;
                            leaveApply.LeaveApprovedTimestamp = DateTime.Now;
                            _db.leaveApplications.Update(leaveApply);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (iSecondLevelAppprovalAuthority == Convert.ToInt32(userId))
                        {
                            LeaveApplication? leaveApply = _db.leaveApplications.FirstOrDefault(a => a.id == id);
                            if (leaveApply != null)
                            {
                                leaveApply.iApproved1 = 1;
                                leaveApply.iApproved2 = 1;
                                if (string.IsNullOrEmpty(leaveApply.sApprovedBy1))
                                    leaveApply.sApprovedBy1 = userName;
                                leaveApply.sApprovedBy2 = userName;
                                if (string.IsNullOrEmpty(leaveApply.ApprovalRemarks1))
                                    leaveApply.ApprovalRemarks1 = remarks;
                                leaveApply.ApprovalRemarks2 = remarks;
                                leaveApply.LeaveApprovedTimestamp = DateTime.Now;
                                _db.leaveApplications.Update(leaveApply);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    if (iSecondLevelAppprovalAuthority != Convert.ToInt32(userId))
                    {
                        LeaveApplication? leaveApply = _db.leaveApplications.FirstOrDefault(a => a.id == id);
                        if (leaveApply != null)
                        {
                            leaveApply.iApproved1 = -1;
                            leaveApply.iApproved2 = -1;
                            leaveApply.sApprovedBy1 = userName;
                            leaveApply.sApprovedBy1 = userName;
                            leaveApply.ApprovalRemarks1 = remarks;
                            leaveApply.LeaveApprovedTimestamp = DateTime.Now;
                            _db.leaveApplications.Update(leaveApply);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (iSecondLevelAppprovalAuthority == Convert.ToInt32(userId))
                        {
                            LeaveApplication? leaveApply = _db.leaveApplications.FirstOrDefault(a => a.id == id);
                            if (leaveApply != null)
                            {
                                if (leaveApply.iApproved1 == 0)
                                    leaveApply.iApproved1 = -1;
                                leaveApply.iApproved2 = -1;
                                if (string.IsNullOrEmpty(leaveApply.sApprovedBy1))
                                    leaveApply.sApprovedBy1 = userName;
                                leaveApply.sApprovedBy2 = userName;
                                if (string.IsNullOrEmpty(leaveApply.ApprovalRemarks1))
                                    leaveApply.ApprovalRemarks1 = remarks;
                                leaveApply.ApprovalRemarks2 = remarks;
                                leaveApply.LeaveApprovedTimestamp = DateTime.Now;
                                _db.leaveApplications.Update(leaveApply);
                                _db.SaveChanges();
                            }
                        }
                    }

                }
                // This action can be used to approve the leave application.

                return Json(new { success = true, message = "Leave approved successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in ApproveLeave: {ex.Message}");
                // Optionally, you can return an error view or message
                return Json(new { success = false, message = "Exception in handle leave appication: " + ex.Message });
            }
        }
    }
}
