using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class MachineMapController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string? database = string.Empty;
        public MachineMapController(ApplicationDBContext db,IUserActivityLogger userActivityLogger)
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
        public async Task<ActionResult> MachineMapPage()
        {
            try
            {
                GenericFunction.WriteLog("EMSSolution", "MachineMapPage Hits");

                #region check if Session Live or Expired
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Login", "Account");
                }
                #endregion

                GenericFunction.WriteLog("EMSSolution", "MachineMapPage userActivity Starts");
                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                   HttpContext.Session.GetString("UserName")
                   , "MachineMap", "MachineMapPage", "user opened Machine Map page");
                GenericFunction.WriteLog("EMSSolution", "MachineMapPage userActivity Ends");


                List<MachineMap> machineMaps = _db.MachineMaps.ToList();
                return View(machineMaps);
            }
            catch (Exception ex)
            {
                GenericFunction.WriteLog("EMSSolution", "MachineMapPage: Exception: " + ex.Message);
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveMachineMap([FromBody] MachineMap machineMap)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                //get next masterid for new entry
                if (machineMap.iMasterid != 0)
                {
                    var machineExist = _db.MachineMaps.FirstOrDefault(c => c.iMasterid != machineMap.iMasterid
                    && c.MachineId == machineMap.MachineId);
                    if (machineExist != null)
                    {
                        return this.Json(new { status = false, message = "Serial No already exist", isNew = 0, data = "" });
                    }
                    machineExist = null;
                }
                else
                {
                    var machineExist = _db.MachineMaps.FirstOrDefault(c => c.MachineId == machineMap.MachineId);
                    if (machineExist != null)
                    {
                        return this.Json(new { status = false, message = "Serial No already exist", isNew = 0, data = "" });
                    }
                    machineExist = null;
                }

                if (machineMap.iMasterid == 0)
                {
                    
                    var isRowExist = _db.MachineMaps.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.MachineMaps.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }
                    machineMap.iMasterid = maxMasterId;
                    _db.MachineMaps.Add(machineMap);
                    _db.SaveChanges();
                    _db.Dispose();
                    
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                       HttpContext.Session.GetString("UserName")
                       , "MachineMap", "SaveMachineMap", "user added Machine: " + machineMap.IPAddress);

                    strMessage = "Machine Mapping added successfully";
                }
                else
                {
                    var machineExist = _db.MachineMaps.FirstOrDefault(c => c.iMasterid == machineMap.iMasterid);
                    machineMap.CreatedDate = machineExist.CreatedDate;
                    _db.Entry(machineExist).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    _db.MachineMaps.Update(machineMap);
                    _db.SaveChanges();
                    iNew = 0;
                    _db.Dispose();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                       HttpContext.Session.GetString("UserName")
                       , "MachineMap", "SaveMachineMap", "user updated Machine: " + machineMap.IPAddress);
                    strMessage = "Machine Mapping updated successfully";
                }

                List<MachineMap> lstMachineMap = new List<MachineMap>();
                lstMachineMap.Add(machineMap);

                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstMachineMap });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }

        }
        [HttpPost]
        public async Task< ActionResult> DeleteMachineMap(int iMasterid)
        {
            string strMessage = "";
            try
            {
                var machineMapData = _db.MachineMaps.FirstOrDefault(c => c.iMasterid == iMasterid);
                if (machineMapData != null)
                {
                    _db.MachineMaps.Remove(machineMapData);
                    _db.SaveChanges();

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                      HttpContext.Session.GetString("UserName")
                      , "MachineMap", "DeleteMachineMap", "user deleted Machine: " + machineMapData.IPAddress);

                    strMessage = "Machine Mapping deleted successfully";
                }
                else
                {
                    strMessage = "Machine Mapping not found";
                }
                return this.Json(new { status = true, message = strMessage });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }

        }

    }
}
