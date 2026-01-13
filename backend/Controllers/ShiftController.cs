using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class ShiftController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        public ShiftController(ApplicationDBContext db)
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
        public ActionResult ShiftPage()
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

                List<Shift> shifts = _db.Shifts.ToList();
                return View(shifts);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult SaveShift([FromBody] Shiftdata shift)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                if (shift == null)
                {
                    return this.Json(new { status = false, message = "getting blank value", isNew = 0, data = "" });
                }
               
                if (shift.shiftdata.iMasterid != 0)
                {
                    var shiftExist = _db.Shifts.FirstOrDefault(c => c.iMasterid != shift.shiftdata.iMasterid
                    && c.sShiftCode == shift.shiftdata.sShiftCode);
                    if (shiftExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    shiftExist = null;
                }
                else
                {
                    var shiftExist = _db.Shifts.FirstOrDefault(c => c.sShiftCode == shift.shiftdata.sShiftCode);
                    if (shiftExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                }
                if (shift.shiftdata.iMasterid == 0)
                {
                    int maxMasterId = 1;
                    var isRowExist = _db.Shifts.FirstOrDefault();
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Shifts.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }
                    shift.shiftdata.iMasterid = maxMasterId;
                    _db.Shifts.Add(shift.shiftdata);
                    iNew = 1;
                    strMessage = "Shift added successfully";
                }
                else
                {
                    _db.Shifts.Update(shift.shiftdata);
                    iNew = 0;
                    strMessage = "Shift updated successfully";
                }
                _db.SaveChanges();

                //Delete existing weekend data

                var existingWeekendData = _db.shiftWeekendDatas.Where(c => c.iShiftid == shift.shiftdata.iMasterid).ToList();
                if(existingWeekendData != null)
                {
                    _db.shiftWeekendDatas.RemoveRange(existingWeekendData);
                    _db.SaveChanges();
                }

                
                //Insert new weekend data
                if (shift.shiftWeekendData != null && shift.shiftWeekendData.Count > 0)
                {
                    foreach (var weekend in shift.shiftWeekendData)
                    {
                        if (weekend.week1Selected == true || weekend.week2Selected == true
                            || weekend.week3Selected == true || weekend.week4Selected == true
                            || weekend.week5Selected == true)
                        {
                            weekend.iShiftid = shift.shiftdata.iMasterid;
                            _db.shiftWeekendDatas.Add(weekend);
                            _db.SaveChanges();
                            //detach the entity to avoid tracking issues
                            _db.Entry(weekend).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        }
                    }
                }
                List<Shift> lstShift = new List<Shift>();
                lstShift.Add(shift.shiftdata);
                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstShift });
                
            }
            catch (Exception ex)
            {
                return this.Json(new { status = true, message = ex.Message, isNew = iNew, data = "" });

            }
        }

        [HttpPost]
        public ActionResult DeleteShift(int iMasterId)
        {
            string strMessage = "";
            try
            {
                var shift = _db.Shifts.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (shift != null)
                {
                    _db.Shifts.Remove(shift);
                    _db.SaveChanges();
                    strMessage = "Shift deleted successfully";

                    var existingWeekendData = _db.shiftWeekendDatas.Where(c => c.iShiftid == iMasterId).ToList();
                    if (existingWeekendData != null)
                    {
                        _db.shiftWeekendDatas.RemoveRange(existingWeekendData);
                        _db.SaveChanges();
                    }
                }
                else
                {
                    strMessage = "Shift not found";
                }
                return this.Json(new { status = true, message = strMessage });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }
        }

        //get shiftwise weekend data
        public ActionResult GetShiftWeekendData(int iShiftId)
        {
            try
            {
                var weekendData = _db.shiftWeekendDatas.Where(c => c.iShiftid == iShiftId).ToList();
                return this.Json(new { status = true, data = weekendData });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }
        }



        
    }
}
