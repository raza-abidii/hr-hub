using EMSSolution.DatabaseAccessLayer;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data.SqlClient;

namespace EMSSolution.Controllers
{
    public class BackupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private readonly string _backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups");
        private readonly IConfiguration _configuration;
        private readonly IUserActivityLogger? _userActivityLogger;
        private readonly string? _connectionString;
        private string strError = string.Empty;
        private string? database = string.Empty;
        public BackupController(IConfiguration configuration, IUserActivityLogger userActivityLogger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!Directory.Exists(_backupFolder))
                Directory.CreateDirectory(_backupFolder);
            _userActivityLogger = userActivityLogger;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        public IActionResult CreateBackup()
        {
            GenericFunction.WriteLog("BackupController", "CreateBackup called");
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion
            return View();
        }

        [HttpGet]
        public IActionResult getBackup()
        {
            try
            {
                GenericFunction.WriteLog("BackupController", "getBackup called");
                //string dbName = "EmpAttendance";

                //get databse name from configuration appsettings.json defualt connection string 
                var builder = new SqlConnectionStringBuilder(_connectionString);
                string dbName = builder.InitialCatalog;


                string fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string backupPath = Path.Combine(_backupFolder, fileName);

                // Replace with your actual connection string
                string? connectionString = _connectionString;

                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupPath}' WITH FORMAT";


                DataLayer dal = new DataLayer();
                dal.GetExecute(sql, ref strError,database);
                //using (SqlConnection conn = new SqlConnection(connectionString))
                //{
                //    conn.Open();
                //    using (SqlCommand cmd = new SqlCommand(sql, conn))
                //    {
                //        cmd.ExecuteNonQuery();
                //    }
                //}

                string? downloadUrl = Url.Action("DownloadBackup", "Backup", new { fileName = fileName });
                return Json(new { success = true, downloadUrl });
            }
            catch (Exception ex)
            {
                GenericFunction.WriteLog("BackupController", "getBackup Exception: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> DownloadBackup(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_backupFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "Backup", "DownloadBackup", "backup downloladed, back filename: " + fileName);

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                GenericFunction.WriteLog("BackupController", "DownloadBackup Exception: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
