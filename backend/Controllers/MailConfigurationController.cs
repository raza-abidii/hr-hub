using EMSSolution.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class MailConfigurationController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        public MailConfigurationController(ApplicationDBContext db)
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
        public IActionResult EmailConfig()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            var emailConfig = _db.emailConfigurationModels.FirstOrDefault();
            return View(emailConfig);
        }
        [HttpPost]
        public IActionResult SaveSMTP(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, bool smtpSsl)
        {
            try
            {
                _db.emailConfigurationModels.RemoveRange(_db.emailConfigurationModels); // Clear existing configurations
                _db.SaveChanges();


                var emailConfig = _db.emailConfigurationModels.FirstOrDefault() ?? new Models.EmailConfigurationModel();
                emailConfig.EmailType = "smtp";
                emailConfig.SmtpHost = smtpHost;
                emailConfig.SmtpPort = smtpPort;
                emailConfig.SmtpUsername = smtpUsername;
                emailConfig.SmtpPassword = smtpPassword;
                emailConfig.SmtpSsl = smtpSsl;
                //if (emailConfig == null)

                _db.emailConfigurationModels.Add(emailConfig);

                _db.SaveChanges();
                return Json(new { success = true, message = "SMTP configuration saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving SMTP configuration: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveOutlook(string outlookEmail, string outlookPassword)
        {
            try
            {
                var emailConfig = _db.emailConfigurationModels.FirstOrDefault() ?? new Models.EmailConfigurationModel();
                emailConfig.EmailType = "outlook";
                emailConfig.outlookEmail = outlookEmail;
                emailConfig.outlookPassword = outlookPassword;
                if (emailConfig == null)
                {
                    _db.emailConfigurationModels.Add(emailConfig);
                }
                else
                {
                    _db.emailConfigurationModels.Update(emailConfig);
                }
                _db.SaveChanges();
                return Json(new { success = true, message = "Outlook configuration saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving Outlook configuration: " + ex.Message });
            }
        }

    }
}
