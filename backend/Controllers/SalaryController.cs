using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class SalaryController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        public SalaryController(ApplicationDBContext db)
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

        public IActionResult EarningDeduction()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            List<EarningDeduction> earnings = _db.earningDeductions.Where(e => e.iType == 0).ToList(); // Get all earnings
            List<EarningDeduction> deductions = _db.earningDeductions.Where(e => e.iType == 1).ToList(); // Get all deductions
            ViewBag.Earnings = earnings;
            ViewBag.Deductions = deductions; 

            return View();
        }

        [HttpPost]
        public IActionResult SaveSalaryDetails(string[] earnings, string[] deductions)
        {
            try
            {
                _db.earningDeductions.RemoveRange(_db.earningDeductions); // Clear existing records
                _db.SaveChanges();                                                       // Save earnings
                foreach (var earning in earnings)
                {
                    if (!string.IsNullOrEmpty(earning))
                    {
                        var newEarning = new Models.EarningDeduction
                        {
                            iType = 0,
                            TypeName = earning,
                        };
                        _db.earningDeductions.Add(newEarning);
                    }
                }
                // Save deductions
                foreach (var deduction in deductions)
                {
                    if (!string.IsNullOrEmpty(deduction))
                    {
                        var newDeduction = new Models.EarningDeduction
                        {
                            iType = 1, // Assuming 1 represents Deduction
                            TypeName = deduction,
                        };
                        _db.earningDeductions.Add(newDeduction);
                    }
                }
                
                _db.SaveChanges(); // Save all changes to the database
                return Json(new { success = "true", message = "Record save successfully" }); // Redirect to index or another view after saving
            }
            catch(Exception ex)
            {
                // Log the exception (optional)
                return Json(new { success = "false", message = "Error saving record: " + ex.Message });
            }
        }


        public IActionResult EmployeeSalaryDefinition()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            var employees = _db.Employees.Where(a => a.bPermanent == true).ToList();
            List<EarningDeduction> earnings = _db.earningDeductions.Where(e => e.iType == 0).ToList(); // Get all earnings
            List<EarningDeduction> deductions = _db.earningDeductions.Where(e => e.iType == 1).ToList(); // Get all deductions
            ViewBag.Earnings = earnings;
            ViewBag.Deductions = deductions;

            ViewBag.Employees = employees;
            return View();
        }

        [HttpGet]
        public IActionResult getSalaryDetail(int iEmployee)
        {
            try
            {
                List<EmployeeSalaryModel> employeeSalary =_db.employeeSalaries.Where(e => e.iEmployeeId == iEmployee).ToList();
                return Json(new { success = "true",message="",salaryDetail= employeeSalary });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return Json(new { success = "false", message = "Error retrieving salary details: " + ex.Message , salaryDetail = ""});
            }
        }

        [HttpPost]
        public IActionResult SaveEmployeeSalary([FromBody] EmployeeSalaryData model)
        {
            try
            {
                // Clear existing salary details for the employee
                var existingSalary = _db.employeeSalaries.Where(s => s.iEmployeeId == model.EmployeeId).ToList();
                if (existingSalary != null)
           
                {
                    //remove all existing salary records for the employee
                    _db.employeeSalaries.RemoveRange(existingSalary);
                    //_db.employeeSalaries.Remove(existingSalary);
                    _db.SaveChanges();
                }

               for(int i = 0; i < model.EarnDeduct.Count; i++)
               {
                    var earnDed = model.EarnDeduct[i];
                    if (earnDed.Amount > 0) // Only save if amount is greater than 0
                    {
                        var salaryModel = new EmployeeSalaryModel
                        {
                            iEmployeeId = model.EmployeeId,
                            iEarningDeductionType = earnDed.iType,
                            EarningDeductionTypeName = earnDed.TypeName,
                            Amount = (double)earnDed.Amount
                        };
                        _db.employeeSalaries.Add(salaryModel); // Add the salary record to the database
                    }
                }
                // Create a new salary record
                _db.SaveChanges(); // Save all changes to the database
                return Json(new { success = "true", message = "Employee salary saved successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return Json(new { success = "false", message = "Error saving employee salary: " + ex.Message });
            }
        }

       
    }
}
