using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace EMSSolution.Controllers
{
    public class LeaveApprovalController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;

        public LeaveApprovalController(ApplicationDBContext db)
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
        public IActionResult Approve(string token)
        {
            try
            {
                GenericFunction.WriteLog("LeaveApprovalController", "Approve: Token: " + token);
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("SFONEMSSolution_EncryptedJWTToken_");
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);


                int requestId = Convert.ToInt32(principal.FindFirst("requestId")?.Value);
                var email = principal.FindFirst("emailid")?.Value;
                var leaveapply = _db.leaveApplications.FirstOrDefault(x => x.id == requestId);

                var preferences = _db.Preferences.FirstOrDefault();
                int iSecondLevelAppprovalAuthority = 0;
                string SecondLevelEmailid = "";
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                    SecondLevelEmailid = Convert.ToString(preferences.secLvlLeaveAppUserMail);
                }

                if (leaveapply != null)
                {
                    if (email == SecondLevelEmailid)
                    {
                        if (leaveapply.iApproved1 >= 0 && leaveapply.iApproved2 == 0)
                        {
                            leaveapply.iApproved1 = 1;
                            leaveapply.iApproved2 = 1;
                            if (leaveapply.iApproved1 != 1)
                                leaveapply.sApprovedBy1 = "";
                            leaveapply.sApprovedBy2 = email;
                            if (leaveapply.iApproved1 != 1)
                                leaveapply.ApprovalRemarks1 = "";
                            leaveapply.ApprovalRemarks2 = "Approved by HR via Mail";
                            leaveapply.LeaveApprovedTimestamp = DateTime.Now;
                            _db.leaveApplications.Update(leaveapply);
                            _db.SaveChanges();
                        }
                        else if (leaveapply.iApproved2 == -1)
                        {
                            return new JsonResult(new { message = "Leave Rejected can not be approved." });
                        }
                        else if (leaveapply.iApproved2 == 1)
                        {
                            return new JsonResult(new { message = "Leave Already approved." });
                        }
                    }

                    else if (leaveapply.iApproved1 == 0)
                    {
                        leaveapply.iApproved1 = 1;
                        leaveapply.sApprovedBy1 = email;
                        leaveapply.ApprovalRemarks1 = "Approved by : " + email;
                        leaveapply.LeaveApprovedTimestamp = DateTime.Now;
                        _db.leaveApplications.Update(leaveapply);
                        _db.SaveChanges();
                    }
                    else if (leaveapply.iApproved1 == -1)
                    {
                        return new JsonResult(new { message = "Leave Rejected can not be approved." });
                    }
                    else
                    {
                        return new JsonResult(new { message = "Leave Already approved." });
                    }
                    return new JsonResult(new { message = "Leave approved successfully." });
                }
                else
                {
                    return new JsonResult(new { message = "No Leave exist to Approval." });

                }
            }
            catch (SecurityTokenValidationException ex)
            {
                // Handle token validation failure
                GenericFunction.WriteLog("LeaveApprovalController", "Approve: Exception: " + ex.Message);
                return BadRequest(new { message = "Invalid token." });
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, new { message = "Exception in Approve: " + ex.Message });
            }
        }

        public IActionResult Reject(string token)
        {
            try
            {
                GenericFunction.WriteLog("LeaveApprovalController", "Reject: Token: " + token);

                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("SFONEMSSolution_EncryptedJWTToken_");
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);


                int requestId = Convert.ToInt32(principal.FindFirst("requestId")?.Value);
                var email = principal.FindFirst("emailid")?.Value;
                var leaveapply = _db.leaveApplications.FirstOrDefault(x => x.id == requestId);

                var preferences = _db.Preferences.FirstOrDefault();
                int iSecondLevelAppprovalAuthority = 0;
                string SecondLevelEmailid = "";
                if (preferences != null)
                {
                    iSecondLevelAppprovalAuthority = Convert.ToInt32(preferences.secLvlLeaveAppUser);
                    SecondLevelEmailid = Convert.ToString(preferences.secLvlLeaveAppUserMail);
                }

                if (leaveapply != null)
                {

                    if (email == SecondLevelEmailid)
                    {
                        if (leaveapply.iApproved1 >= 0 && leaveapply.iApproved2 == 0)
                        {
                            leaveapply.iApproved1 = -1;
                            leaveapply.iApproved2 = -1;
                            //leaveapply.sApprovedBy1 = "";
                            leaveapply.sApprovedBy2 = email;
                            leaveapply.ApprovalRemarks1 = "";
                            leaveapply.ApprovalRemarks2 = "Rejected by HR via Mail";
                            leaveapply.LeaveApprovedTimestamp = DateTime.Now;
                            _db.leaveApplications.Update(leaveapply);
                            _db.SaveChanges();
                        }
                        else if (leaveapply.iApproved2 == -1)
                        {
                            return new JsonResult(new { message = "Leave already Rejected." });
                        }
                        else if (leaveapply.iApproved2 == 1)
                        {
                            return new JsonResult(new { message = "Leave Already approved." });
                        }
                    }

                    else if (leaveapply.iApproved1 == 0)
                    {
                        leaveapply.iApproved1 = -1;
                        leaveapply.iApproved2 = -1;
                        leaveapply.sApprovedBy1 = email;
                        leaveapply.sApprovedBy2 = "";
                        leaveapply.ApprovalRemarks1 = "Rejected by " + email;
                        leaveapply.LeaveApprovedTimestamp = DateTime.Now;
                        _db.leaveApplications.Update(leaveapply);
                        _db.SaveChanges();
                    }
                    else if (leaveapply.iApproved1 == -1)
                    {
                        return new JsonResult(new { message = "Leave Already Rejected" });
                    }
                    else
                    {
                        return new JsonResult(new { message = "Leave Already approved." });
                    }
                    return new JsonResult(new { message = "Leave Reject successfully." });
                }
                else
                {
                    return new JsonResult(new { message = "No Leave exist to Reject." });
                }

            }
            catch (SecurityTokenValidationException ex)
            {
                // Handle token validation failure
                GenericFunction.WriteLog("LeaveApprovalController", "Reject: Exception: " + ex.Message);
                return BadRequest(new { message = "Invalid token." });
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                GenericFunction.WriteLog("LeaveApprovalController", "Reject: Exception: " + ex.Message);
                return StatusCode(500, new { message = "Exception in Reject: " + ex.Message });
            }
        }
    }
}
