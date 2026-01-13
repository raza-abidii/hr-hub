using DocumentFormat.OpenXml.Office2010.PowerPoint;
using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace EMSSolution.Controllers
{
    public class LeaveApplicationController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IConfiguration _configuration;
        string database = string.Empty;
        public LeaveApplicationController(ApplicationDBContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
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
        //LeaveApply
        public IActionResult LeaveApply([FromBody] LeaveRequestModel leaveRequestModel)
        {
            try
            {
                GenericFunction.WriteLog("LeaveApply", "LeaveApply Controller Hits");
                if (leaveRequestModel == null)
                {
                    return BadRequest("Invalid leave request model.");
                }
                var date = leaveRequestModel.datefrom;
                //self join of employee table to get reporting manager email

                var reportingTo = (from emp in _db.Employees
                                   join emp1 in _db.Employees on emp.iReportingTo equals emp1.iMasterid
                                   where emp.iMasterid == leaveRequestModel.iEmployee
                                   select new { emp1.sEmailId, emp1.iMasterid }).FirstOrDefault();

                var preferences= _db.Preferences.FirstOrDefault();

                int iSecondLevelAppprovalAuthority = 0;
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority =Convert.ToInt32( preferences.secLvlLeaveAppUser);
                }

                var leaveAlreadyTakenInDateRange = _db.leaveApplications.Where(
                            l => l.iEmployee == leaveRequestModel.iEmployee
                            && (l.dFromDate >= leaveRequestModel.datefrom && l.dFromDate <= leaveRequestModel.dateto
                            || l.dToDate >= leaveRequestModel.datefrom && l.dToDate <= leaveRequestModel.dateto))
                            .Select(l => new { l.iEmployee })
                            .ToList();
                if (leaveAlreadyTakenInDateRange != null && leaveAlreadyTakenInDateRange.Count > 0)
                {
                    return Json(new { success = false, message = "Leave already taken in the selected date range." });
                }


                var attendanceAlreadyExistInDateRange =
                    (from ts in _db.EmployeeTimeSheets
                     join emp in _db.Employees
                     on ts.EmpId equals emp.sEmployeeCode
                     where (leaveRequestModel.datefrom >= ts.LogDateTime.Date && leaveRequestModel.datefrom <= ts.LogDateTime.Date
                     || leaveRequestModel.dateto >= ts.LogDateTime.Date && leaveRequestModel.dateto <= ts.LogDateTime.Date)
                     && emp.iMasterid == leaveRequestModel.iEmployee
                     select new
                     {
                         ts.EmpId,
                         emp.sEmployeeName // example of additional joined data
                     }).ToList();

                if (attendanceAlreadyExistInDateRange.Count > 0)
                {
                    return Json(new { success = false, message = "Attendance already exist in the selected date range." });
                }

                foreach (var leaveUsed in leaveRequestModel.leaveUsed)
                {
                    var leave = _db.Leaves.FirstOrDefault(l => l.iMasterid == leaveUsed.LeaveTypeId);
                    if (leave != null)
                    {
                        //if (leaveUsed.UsedDays > 0)
                        for (int ileave = 0; ileave < leaveUsed.UsedDays; ileave++)
                        {
                            var leaveApplication = new LeaveApplication
                            {
                                iEmployee = leaveRequestModel.iEmployee,
                                dFromDate = date,
                                dToDate = date,
                                fTotalDaysTaken = leaveRequestModel.noofdays,
                                iLeaveType = leaveUsed.LeaveTypeId,
                                fDaysTakenOnLeaveType = 1,//leaveUsed.UsedDays,
                                sRemarks = leaveRequestModel.reason,
                                isfullday = Convert.ToBoolean(leaveRequestModel.isfullday) ? true : false,
                                iApprovedAuthority1 = reportingTo != null ? reportingTo.iMasterid : 0, // Set reporting manager as first approver
                                iApprovedAuthority2= iSecondLevelAppprovalAuthority //set second Level HR 
                            };
                            date = date.AddDays(1);
                            _db.leaveApplications.Add(leaveApplication);
                            int iReturn = _db.SaveChanges();
                            GenericFunction.WriteLog("LeaveApply", "Leave Applied: Date: " + leaveApplication.dFromDate);
                            if (iReturn > 0)
                            {
                                //create JWTToken and update into LeaveApplicationId1
                                if (reportingTo != null && !string.IsNullOrEmpty(reportingTo.sEmailId))
                                {
                                    #region if Reporting To Manager Allign
                                    var token = GenerateApprovalToken(leaveApplication.id.ToString(), reportingTo.sEmailId);
                                    leaveApplication.LeaveApplicationId1 = token;
                                    var emailonfig = _db.emailConfigurationModels.FirstOrDefault();
                                    if (emailonfig != null && emailonfig.EmailType == "smtp")
                                    {
                                        string EmailMessage = $"Leave Application from {leaveApplication.dFromDate.ToShortDateString()} " +
                                            $"to {leaveApplication.dToDate.ToShortDateString()}{Environment.NewLine}Remarks: {leaveApplication.sRemarks}";
                                        //send mail to reporting manager
                                        if (sendMailSMTP(emailonfig.SmtpUsername, emailonfig.SmtpPassword
                                            , emailonfig.SmtpPort.ToString(), emailonfig.SmtpHost, reportingTo.sEmailId,
                                            EmailMessage, token))
                                        {
                                            GenericFunction.WriteLog("LeaveApply", "Email sent to reporting manager: " + reportingTo.sEmailId);
                                        }
                                        else
                                        {
                                            GenericFunction.WriteLog("LeaveApply", "Failed to send email to reporting manager: " + reportingTo.sEmailId);
                                        }
                                       

                                    }
                                    else if (emailonfig != null && emailonfig.EmailType != "smtp")
                                    {
                                        //sendMailOutlook
                                    }
                                    else
                                    {
                                        leaveApplication.LeaveApplicationId1 = string.Empty; // If email is not enabled, clear the token

                                    }
                                    #endregion
                                }
                                else
                                {
                                    GenericFunction.WriteLog("LeaveApply", "Email id of reporting manager does not exist");
                                    leaveApplication.iApproved1 = 0; // If no reporting manager found, mark as approved by default
                                }

                                #region if No Reporting Manager/or reporting manager Allign and set HR as Approver2
                                if(preferences!=null && preferences.secLvlLeaveApproval==true && Convert.ToInt32( preferences.secLvlLeaveAppUser)>0 
                                    && !string.IsNullOrEmpty( preferences.secLvlLeaveAppUserMail))
                                {
                                    var token = GenerateApprovalToken(leaveApplication.id.ToString(), preferences.secLvlLeaveAppUserMail);
                                    leaveApplication.LeaveApplicationId2 = token;
                                    var emailonfig = _db.emailConfigurationModels.FirstOrDefault();
                                    if (emailonfig != null && emailonfig.EmailType == "smtp")
                                    {
                                        string EmailMessage = $"Leave Application from {leaveApplication.dFromDate.ToShortDateString()} " +
                                            $"to {leaveApplication.dToDate.ToShortDateString()}{Environment.NewLine}Remarks: {leaveApplication.sRemarks}";
                                        //send mail to reporting manager
                                        if (sendMailSMTP(emailonfig.SmtpUsername, emailonfig.SmtpPassword
                                            , emailonfig.SmtpPort.ToString(), emailonfig.SmtpHost, preferences.secLvlLeaveAppUserMail,
                                            EmailMessage, token))
                                        {
                                            GenericFunction.WriteLog("LeaveApply", "Email sent to reporting manager: " + reportingTo.sEmailId);
                                        }
                                        else
                                        {
                                            GenericFunction.WriteLog("LeaveApply", "Failed to send email to reporting manager: " + reportingTo.sEmailId);
                                        }


                                    }
                                    else if (emailonfig != null && emailonfig.EmailType != "smtp")
                                    {
                                        //sendMailOutlook
                                    }
                                    else
                                    {
                                        leaveApplication.LeaveApplicationId2 = string.Empty; // If email is not enabled, clear the token

                                    }
                                }
                                #endregion

                                _db.leaveApplications.Update(leaveApplication);
                                iReturn = _db.SaveChanges();
                            }

                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Leave type not found." });
                    }
                }
                //_db.SaveChanges();

                return Json(new { success = true, message = "Leave application submitted successfully." });
                //return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while processing the leave application.: " + ex.Message });
            }

        }

        private bool sendMailSMTP(string MailFrom, string password, string smtpPort, string smtpHost,
            string MailTo, string Message, string token)
        {
            //string approvalId = "16"; // your approval item ID
            //string baseUrl = "http://localhost:5175"; // your actual domain or site URL

            //read EmailApprovalURL key from application.json 
            string baseUrl = _configuration["EmailApprovalURL"];


            string approveUrl = $"{baseUrl}/LeaveApproval/Approve?token={token}";
            string rejectUrl = $"{baseUrl}/LeaveApproval/Reject?token={token}";

            //<p><strong>Request ID:</strong> {token}</p>
            string subject = "Approval Request for Leave";
            string body = $@"
                <html>
                <body>
                    <p>Dear Approver,</p>
                    <p>A new request is awaiting your approval. Please review and take action:</p>
                    
                    <p><strong>Message:</strong> {Message}</p>

    
                    <a href='{approveUrl}' style='
                        display: inline-block;
                        padding: 10px 20px;
                        color: white;
                        background-color: green;
                        text-decoration: none;
                        border-radius: 5px;'>Approve</a>

                    &nbsp;&nbsp;

                    <a href='{rejectUrl}' style='
                        display: inline-block;
                        padding: 10px 20px;
                        color: white;
                        background-color: red;
                        text-decoration: none;
                        border-radius: 5px;'>Reject</a>

                    <p>Regards,<br/>Your Company Name</p>
                </body>
                </html>";

            try
            {
                // Send the email using Gmail

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(MailFrom);
                    mail.To.Add(MailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(smtpHost, Convert.ToInt32(smtpPort)))
                    {
                        //smtp.Credentials = new NetworkCredential("focusemailtesting@gmail.com", "focus@email123");
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(MailFrom, password);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
                // Handle any errors that may have occurred
                //MessageBox.Show("Error sending email: " + ex.Message);
            }

        }

        //YourSecretKeyHere_AtLeast_16Chars
        private readonly string _jwtSecret = "SFONEMSSolution_EncryptedJWTToken_"; //16 character Store securely!
        public string GenerateApprovalToken(string requestId, string email)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecret);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("requestId", requestId.ToString()),
                        new Claim("emailid", email.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(3),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return null; // or throw a custom exception
            }
        }

        [HttpPost]
        //GetLeave
        public IActionResult GetLeaveDetail(int employeeId, DateTime dDate)
        {
            try
            {
                List<LeaveDetail> attendanceReportModels = _db.Database.SqlQuery<LeaveDetail>
                       ($@"exec sp_leavedetail {employeeId}, {dDate.Month}, {dDate.Year}").ToList();
                if (attendanceReportModels != null && attendanceReportModels.Count > 0)
                {
                    return Json(new { success = true, data = attendanceReportModels });
                }
                else
                {
                    return Json(new { success = false, message = "No leave details found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while processing the leave application.: " + ex.Message });
            }

        }
    }
}
