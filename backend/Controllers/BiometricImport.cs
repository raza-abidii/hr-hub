using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using EMSSolution.LoggingService;

namespace EMSSolution.Controllers
{
    public class BiometricImport : Controller
    {
        private readonly IConfiguration _configuration;
        readonly ApplicationDBContext _db;
        private readonly HttpClient _httpClient;
        private readonly WebClient _webClient;
        private readonly IUserActivityLogger? _userActivityLogger;
        public IActionResult Index()
        {
            return View();
        }
        public BiometricImport(ApplicationDBContext db, IConfiguration configuration, 
            HttpClient httpClient,WebClient webClient,IUserActivityLogger userActivityLogger)
        {
            _db = db;
            _configuration = configuration;
            _httpClient = httpClient;
            _webClient = webClient;
            _userActivityLogger = userActivityLogger;
        }
        public async Task<ActionResult> BiometricImportPage()
        {
            try
            {
                #region check if Session Live or Expired
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Login", "Account");
                }
                #endregion

                var machinemap = _db.MachineMaps.Where(a => a.IsActive == true).ToList();
                ViewBag.Machinemap = machinemap;

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "BiometricImport", "BiometricImportPage", "opened Biometric Data Page");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error: " + ex.Message, new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public ActionResult AutoSelectMachine(string ipAddress)
        {
            try
            {
                var machine = _db.MachineMaps.FirstOrDefault(m => m.IPAddress == ipAddress);
                if (machine != null)
                {
                    return Json(new { status = true, message = "Success", data = machine.MachineId });
                }
                else
                {
                    return Json(new { status = false, message = "No Machine Map for selected IP", data = "" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Error: " + ex.Message, data = "" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ImportBiometricData(string ipAddress, int machineId, Int64 portNo)
        {
            string strMessage = string.Empty;
            strMessage =ImportBiometricDataEpush(ipAddress, machineId, portNo);

            //strMessage = await callWithWebClient(ipAddress, machineId, portNo);

            //if (!string.IsNullOrEmpty(strMessage) && strMessage.IndexOf("Success") >= 0)
            if (string.IsNullOrEmpty(strMessage) )
            {
                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "BiometricImport", "ImportBiometricData", "Biometric Data Imported");
                return Json(new { status = true, message = "Biometric data imported successfully." + strMessage});
            }
            else
            {
                return Json(new { status = false, message = strMessage });
            }
        }

        [HttpPost]
        public string ImportBiometricDataEpush(string ipAddress, int machineId, Int64 portNo)
        {
            string strReturn = string.Empty;
            try
            {
                
                DatabaseAccessLayer.DataLayer dal = new DatabaseAccessLayer.DataLayer();
                string strError = string.Empty;
                string strQry = string.Empty;
                DataSet ds = new DataSet();
                strQry = "TRUNCATE TABLE tblEmployeeTimeSheet";
                dal.GetExecute(strQry, ref strError);
                for (int imonth = 1; imonth <= DateTime.Now.Month; imonth++)
                {
                    strQry = $@"Select * from epush..DeviceLogs_{imonth}_{DateTime.Now.Year}";
                    ds = dal.GetData(strQry, ref strError);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        strQry = $@"INSERT INTO tblEmployeeTimeSheet (IPAddress, EmpId, EmpName,LogDate,LogTime,logDateTime)
                            SELECT deviceId, UserId,'', format(LogDate,'dd-MM-yyyy'),format(LogDate,'HH:mm')
                            ,logDate
                            FROM epush.dbo.DeviceLogs_{imonth}_{DateTime.Now.Year}";
                        dal.GetExecute(strQry,ref strError);
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                strReturn="Exception: " + ex.Message;
                return strReturn;
            }
        }

        private async Task<string> callWithWebClient(string ipAddress, int machineId, Int64 portNo)
        {
            try
            {
                var baseURL = _configuration["APIURL"];
                var sRequest = new BiometricRequest
                {
                    ipAddress = ipAddress,
                    machineId = machineId,
                    portNo = 8080
                };
                var json = JsonSerializer.Serialize(sRequest);
                
                _webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var response = await _webClient.UploadStringTaskAsync(new Uri(baseURL + $@"PostBiometricData"), "POST", json);
                return "Success:" + response;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private async Task<string> callWithHttplClient(string ipAddress, int machineId, Int64 portNo)
        {
            try
            {
                var baseURL = _configuration["APIURL"];
                var sRequest = new BiometricRequest
                {
                    ipAddress = ipAddress,
                    machineId = machineId,
                    portNo = 8080
                };
                var json = JsonSerializer.Serialize(sRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(baseURL + $@"PostBiometricData", content);
                // Call the method to import biometric data here
                // You can use the machineId, startDate, and endDate parameters as needed
                // Example: ImportBiometricDataFromMachine(machineId, startDate, endDate);
                //return Json(new { status = true, message = "Biometric data imported successfully." });
                return "Success:" + response.StatusCode;
            }
            catch (Exception ex)
            {
                return ex.Message;
                //return Json(new { status = false, message = "Error: " + ex.Message });
            }
        }
    }
}
