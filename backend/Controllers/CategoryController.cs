using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;

namespace EMSSolution.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        public string? UserName { get; set; } = string.Empty;
        public string database = string.Empty;
        public CategoryController(ApplicationDBContext db, IUserActivityLogger? userActivityLogger)
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
        public async Task< ActionResult> CategoryPage()
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
                    , "Category", "CategoryPage","User opened Category Page");

                UserName = HttpContext.Session.GetString("UserName");
                List<Category> categories = _db.Categories.ToList();
                return View(categories);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveCategory([FromBody] Category category)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                //get next masterid for new entry
                if (category.iMasterid != 0)
                {
                    var categoryExist = _db.Categories.FirstOrDefault(c => c.iMasterid != category.iMasterid 
                    && c.sCode==category.sCode );
                    if (categoryExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    categoryExist = null;
                }
                else
                {
                    var categoryExist = _db.Categories.FirstOrDefault(c => c.sCode ==category.sCode);
                    if (categoryExist != null)
                    {
                        return this.Json(new { status = false, message = "Code already exist", isNew = 0, data = "" });
                    }
                    categoryExist = null;
                }
                
                if (category.iMasterid == 0)
                {
                    //categoryData = new Category();
                    var isRowExist = _db.Categories.FirstOrDefault();
                    int maxMasterId = 1;
                    if (isRowExist != null)
                    {
                        maxMasterId = _db.Categories.Max(c => c.iMasterid);
                        maxMasterId = maxMasterId + 1;
                    }
                  
                    category.iMasterid = maxMasterId;    
                    _db.Categories.Add(category);
                    _db.SaveChanges();
                    //_db.Dispose();
                    strMessage = "Category added successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName"), "Category", "SaveCategory", "User Saved Category: " + category.sCode);
                }
                else
                {
                    _db.Categories.Update(category);
                    _db.SaveChanges();
                    iNew = 0;
                    //_db.Dispose();
                    strMessage = "Category updated successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName"), "Category", "SaveCategory", "User Updated Category: " + category.sCode);
                }

                List<Category> lstCategory = new List<Category>();
                lstCategory.Add(category);

                

                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstCategory });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCategory(int iMasterId)
        {
            try
            {
                var category = _db.Categories.FirstOrDefault(c => c.iMasterid == iMasterId);
                if (category != null)
                {
                    var empExist = _db.Employees.FirstOrDefault(c => c.iCategory == iMasterId);
                    if (empExist != null)
                    {
                        return this.Json(new { status = false, message = "Category Already mapped to Employee, can not delete" });
                    }

                    _db.Categories.Remove(category);
                    _db.SaveChanges();
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                        HttpContext.Session.GetString("UserName"), "Category", "DeleteCategory", 
                        "User Deleted Category: " + category.sCode);
                    return this.Json(new { status = true, message = "Category deleted successfully" });
                }
                else
                {
                    return this.Json(new { status = false, message = "Category not found" });
                }
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message });
            }
        }
    }
}
