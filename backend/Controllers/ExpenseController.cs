using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IUserActivityLogger? _userActivityLogger;
        string database = string.Empty;
        public ExpenseController(ApplicationDBContext db, IUserActivityLogger userActivityLogger)
        {
            _userActivityLogger = userActivityLogger;
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
        public async Task<IActionResult> Expenses()
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

                await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                    , HttpContext.Session.GetString("UserName")
                    , "Expense", "Expenses", "Expense Page Opened");


                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
                    ViewBag.Employees = employees;
                }
                else
                {
                    var employees = _db.Employees.Where(a => a.bPermanent == true
                        && a.iMasterid == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"))).ToList();

                    ViewBag.Employees = employees;
                }
                if (HttpContext.Session.GetString("Role") == "Admin")
                {
                    var Expenses = _db.Expenses.Where(a => a.ApprovalStatus == 0);
                    ViewBag.Expenses = Expenses;
                }
                else
                {
                    var Expenses = _db.Expenses.Where(a => a.iEmployee == Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"))
                    && a.ApprovalStatus == 0).ToList();

                    ViewBag.Expenses = Expenses;
                }
                return View();
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpPost]
        public IActionResult DeleteExpense(int id)
        {
            try
            {
                var expense = _db.Expenses.Find(id);
                if (expense != null)
                {
                    _db.Expenses.Remove(expense);
                    _db.SaveChanges();
                }
                return RedirectToAction("Expenses");
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpPost]
        public IActionResult AddExpense([FromBody]  Expenses model)
        {
            try
            {
                int iNew = 1;
                if (model!=null)
                {
                    if (model.id == 0)
                    {
                        if (model.sImage == null)
                            model.sImage = "data:,";
                        model.CreatedDate = DateTime.Now;
                        _db.Expenses.Add(model);
                    }
                    else
                    {
                        if (model.sImage == null)
                            model.sImage = "data:,";
                        _db.Expenses.Update(model);
                        iNew = 0;
                    }
                        // Save logic here
                        _db.SaveChanges();
                }

                List<Expenses> lstExpense= new List<Expenses>();
                lstExpense.Add(model);
                return this.Json(new { success = true, message = "expense added successfully", isNew = iNew, data = lstExpense });
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine(ex.Message);
                return this.Json(new { success = false, message = ex.Message, isNew = 0, data = "" });
            }
        }

        [HttpPost]
        public IActionResult SaveExpense(Expenses model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.id == 0)
                        _db.Expenses.Add(model);
                    else
                        _db.Expenses.Update(model);
                    // Save logic here
                    _db.SaveChanges();
                }
                return RedirectToAction("Expenses");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        public IActionResult HandleExpenseAction(int id, string actionType, string remarks,string Amount)
        {
            try
            {
                int userId =Convert.ToInt32( HttpContext.Session.GetString("UserId"));
                if (actionType == "Approve")
                {
                    Expenses? expenses = _db.Expenses.FirstOrDefault(a => a.id == id);
                    expenses.ApprovalStatus = 1;
                    expenses.ApprovedBy = userId;
                    expenses.Remarks = remarks;
                    expenses.ApprovedDate = DateTime.Now;
                    expenses.ApprovedAmount =Convert.ToDouble( Amount);

                    _db.Expenses.Update(expenses);
                    _db.SaveChanges();
                }
                else
                {
                    Expenses? expenses = _db.Expenses.FirstOrDefault(a => a.id == id);
                    expenses.ApprovalStatus = -1;
                    expenses.ApprovedBy = userId;
                    expenses.Remarks = remarks;
                    expenses.ApprovedDate = DateTime.Now;
                    _db.Expenses.Update(expenses);
                    _db.SaveChanges();

                }
                // This action can be used to approve the leave application.

                return Json(new { success = true, message = "Leave approved successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in ApproveLeave: {ex.Message}");
                // Optionally, you can return an error view or message
                return Json(new { success = false, message = "An error occurred while approving the leave." });
            }
        }

    }
}
