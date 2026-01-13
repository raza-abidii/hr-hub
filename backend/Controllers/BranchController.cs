using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class BranchController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        public string? UserName { get; set; } = string.Empty;
        public string? database = string.Empty;
        public BranchController(ApplicationDBContext db, IUserActivityLogger? userActivityLogger)
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
        public async Task<ActionResult> BranchPage()
        {
            try
            {
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
                if (isAuthenticated != "true")
                {
                    return RedirectToAction("Account", "Login");
                }

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                    HttpContext.Session.GetString("UserName")
                    , "Branch", "BranchPage", "User opened Branch Page");

                UserName = HttpContext.Session.GetString("UserName");
                List<Branch> branches = _db.Branches.ToList();
                return View(branches);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveBranch([FromBody] Branch branch)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                Branch? branchExist;
                //get next masterid for new entry
                if (branch.iMasterid != 0)
                {
                    branchExist = _db.Branches.FirstOrDefault(c => c.iMasterid != branch.iMasterid
                        && c.sCode == branch.sCode);
                    if (branchExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    branchExist = null;
                }
                else
                {
                    branchExist = _db.Branches.FirstOrDefault(c => c.sCode == branch.sCode);
                    if (branchExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    branchExist = null;
                }

                if (branch.iMasterid == 0)
                {
                    var isRowExist = _db.Branches.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Branches.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }

                    branch.iMasterid = maxMasterId;
                    _db.Branches.Add(branch);
                    _db.SaveChanges();
                    //_db.Dispose();
                    strMessage = "Branch added successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName"), "Branch", "SaveBranch", "User Saved Branch: " + branch.sCode);
                }
                else
                {
                    _db.Branches.Update(branch);
                    _db.SaveChanges();
                    iNew = 0;
                    //_db.Dispose();
                    strMessage = "Branch updated successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName"), "Branch", "SaveBranch", "User Updated Branch: " + branch.sCode);
                }

                List<Branch> lstBranch = new List<Branch>();
                lstBranch.Add(branch);

                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstBranch });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteBranch(int iMasterId)
        {
            try
            {
                var branch = _db.Branches.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (branch != null)
                {
                    var empExist = _db.Employees.FirstOrDefault(c => c.iBranch == iMasterId);
                    if (empExist!=null)
                    {
                        return this.Json(new { status = false, message = "Branch Already mapped to Employee, can not delete" });
                    }
                    _db.Branches.Remove(branch);
                    _db.SaveChanges();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName"), "Branch", "DeleteBranch",
                        "User Deleted Branch: " + branch.sCode);
                    return this.Json(new { status = true, message = "Branch deleted successfully" });
                }
                else
                {
                    return this.Json(new { status = false, message = "Branch not found" });
                }
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }
        }
    }
}
