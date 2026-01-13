using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace EMSSolution.Controllers
{
    public class ShiftAllocationController : Controller
    {
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        public ShiftAllocationController(ApplicationDBContext context)
        {
            _db = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }
        public IActionResult Create()
        {
            try
            {
                var model = new ShiftAllocationViewModel
                {
                    Employees = GetEmployees(),
                    Categories = GetCategories(),
                    Shifts = GetShifts(),
                    SelectedYear = DateTime.Now.Year,
                    SelectedMonth = DateTime.Now.Month,
                    DaysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error in Create: {ex.Message}");
                // Optionally, you can return an error view or message
                return View("Error", new { message = "An error occurred while loading the page." });
            }
        }
        private List<SelectListItem> GetEmployees()
        {
            var employees = _db.Employees.ToList();
            //convert employee in SelectedListItem
            List<SelectListItem> employeeList = employees.Select(e => new SelectListItem
            {
                Text = e.sEmployeeName,
                Value = e.iMasterid.ToString()
            }).ToList();
            return employeeList;

        }

        private List<SelectListItem> GetCategories()
        {
            var categories = _db.Categories.ToList();
            List<SelectListItem> categoryList = categories.Select(c => new SelectListItem
            {
                Text = c.sName,
                Value = c.iMasterid.ToString()
            }).ToList();
            return categoryList;
        }

        private List<SelectListItem> GetShifts()
        {
            var shifts = _db.Shifts.ToList();
            List<SelectListItem> shiftList = shifts.Select(s => new SelectListItem
            {
                Text = s.sShiftName,
                Value = s.iMasterid.ToString()
            }).ToList();
            return shiftList;

        }
        [HttpPost]
        public IActionResult Create(ShiftAllocationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save logic here
                return RedirectToAction("Success");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult SaveShiftAllocation([FromForm] IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                ShiftAllocationViewPostModel? data = new ShiftAllocationViewPostModel
                {
                    AllocationType = form["AllocationType"],
                    SelectedEmployeeId = string.IsNullOrEmpty(form["SelectedEmployeeId"]) ? null : (int?)Convert.ToInt32(form["SelectedEmployeeId"]),
                    SelectedCategoryId = string.IsNullOrEmpty(form["SelectedCategoryId"]) ? null : (int?)Convert.ToInt32(form["SelectedCategoryId"]),
                    TimeFrameType = form["TimeFrameType"],
                    BulkShiftSelect = string.IsNullOrEmpty(form["BulkShiftSelect"]) ? null : (int?)Convert.ToInt32(form["BulkShiftSelect"]),
                    SelectedMonth = string.IsNullOrEmpty(form["SelectedMonth"]) ? null : (int?)Convert.ToInt32(form["SelectedMonth"]),
                    SelectedYear = string.IsNullOrEmpty(form["SelectedYear"]) ? null : (int?)Convert.ToInt32(form["SelectedYear"])
                };

                if (Convert.ToInt32(data.SelectedEmployeeId) == 0 && Convert.ToInt32(data.SelectedCategoryId) == 0)
                {
                    return this.Json(new { status = false, message = "Either Employee/Category Need to be select", isNew = 0, data = "" });
                }


                // Extract day-wise shifts
                foreach (var key in form.Keys.Where(k => k.StartsWith("Shift_Day_")))
                {
                    if (form[key] == "")
                        data.ShiftDays[key] = 0;
                    else
                        data.ShiftDays[key] = int.Parse(form[key]);
                }

                // Extract month-wise shifts
                foreach (var key in form.Keys.Where(k => k.StartsWith("Shift_Month_")))
                {
                    data.ShiftMonths[key] = int.Parse(form[key]);
                }
                // Save logic here

                if (data.AllocationType == "EmployeeWise")
                {
                    string message = saveEmployeewise(data);
                    if (string.IsNullOrEmpty(message))
                        return this.Json(new { status = true, message = "Employee-wise allocation saved successfully.", isNew = 0, data = "" });
                    else
                        return this.Json(new { status = false, message = message, isNew = 0, data = "" });
                }
                else if (data.AllocationType == "CategoryWise")
                {
                    string message = saveCategorywise(data);
                    if (string.IsNullOrEmpty(message))
                        return this.Json(new { status = true, message = "Category-wise allocation saved successfully.", isNew = 0, data = "" });
                    else
                        return this.Json(new { status = false, message = message, isNew = 0, data = "" });
                }
                else
                {
                    ViewBag.Message = "Invalid allocation type.";
                }

                return RedirectToAction("Success");
            }
            return View(form);
        }

        private string saveEmployeewise(ShiftAllocationViewPostModel data)
        {
            try
            {
                int iCategory = _db.Employees
                    .Where(e => e.iMasterid == data.SelectedEmployeeId)
                    .Select(e => e.iCategory)
                    .FirstOrDefault();
                if (data.TimeFrameType == "Monthly")
                {
                    //get no of days for selected month

                    int daysInMonth = DateTime.DaysInMonth(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month);
                    for (int i = 1; i <= daysInMonth; i++)
                    {
                        //if combination of iemployee and ddate already exist in tblShiftallocation then update the record
                        ShiftAllocation? existingRecord = _db.shiftAllocations
                            .FirstOrDefault(x => x.iEmployee == data.SelectedEmployeeId && x.dDate == new DateTime(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month, i));
                        if (existingRecord == null)
                        {
                            //daywise record save in tblShiftallocation
                            var shiftAllocation = new ShiftAllocation
                            {
                                AllocationType = data.AllocationType,
                                iCategory = iCategory,
                                iEmployee = Convert.ToInt32(data.SelectedEmployeeId),
                                TimeFrame = data.TimeFrameType,
                                iMonth = data.SelectedMonth ?? DateTime.Now.Month,
                                iYear = data.SelectedYear ?? DateTime.Now.Year,
                                iDay = i,
                                dDate = new DateTime(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month, i),
                                iShift = Convert.ToInt32(data.ShiftDays["Shift_Day_" + (i)]),
                            };
                            _db.shiftAllocations.Add(shiftAllocation);
                        }
                        else
                        {
                            //if record not exist then update the record
                            existingRecord.iShift = Convert.ToInt32(data.ShiftDays["Shift_Day_" + (i)]);
                            _db.shiftAllocations.Update(existingRecord);
                        }
                    }
                    _db.SaveChanges();
                }
                else if (data.TimeFrameType == "Yearly")
                {
                    //get no of days for selected month
                    int daysInYear = DateTime.IsLeapYear(data.SelectedYear ?? DateTime.Now.Year) ? 366 : 365;
                    for (int i = 1; i <= daysInYear; i++)
                    {
                        //if combination of iemployee and ddate already exist in tblShiftallocation then update the record
                        ShiftAllocation? existingRecord = _db.shiftAllocations
                            .FirstOrDefault(x => x.iEmployee == data.SelectedEmployeeId && x.dDate == new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1));
                        int iMonth = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Month;
                        if (existingRecord == null)
                        {


                            //daywise record save in tblShiftallocation
                            var shiftAllocation = new ShiftAllocation
                            {
                                AllocationType = data.AllocationType,
                                iCategory = iCategory,
                                iEmployee = Convert.ToInt32(data.SelectedEmployeeId),
                                TimeFrame = data.TimeFrameType,
                                dDate = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1),
                                iDay = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Day,
                                iMonth = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Month,
                                iYear = data.SelectedYear ?? DateTime.Now.Year,
                                iShift = data.ShiftMonths["Shift_Month_" + iMonth],
                            };
                            _db.shiftAllocations.Add(shiftAllocation);
                        }
                        else
                        {
                            //if record not exist then update the record

                            existingRecord.iShift = Convert.ToInt32(data.ShiftMonths["Shift_Month_" + (iMonth)]);
                            _db.shiftAllocations.Update(existingRecord);
                        }
                    }
                    _db.SaveChanges();
                }
                else
                {
                    return "Invalid time frame type.";
                }
                return "";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string saveCategorywise(ShiftAllocationViewPostModel data)
        {
            try
            {
                var iEmployee = _db.Employees
                    .Where(e => e.iCategory == data.SelectedCategoryId)
                    .Select(e => e.iMasterid)
                    .ToList();
                if (data.TimeFrameType == "Monthly")
                {
                    //get no of days for selected month
                    int daysInMonth = DateTime.DaysInMonth(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month);
                    for (int i = 1; i <= daysInMonth; i++)
                    {
                        foreach (var emp in iEmployee)
                        {
                            //if combination of iemployee and ddate already exist in tblShiftallocation then update the record
                            ShiftAllocation? existingRecord = _db.shiftAllocations
                                .FirstOrDefault(x => x.iEmployee == emp && x.dDate == new DateTime(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month, i));
                            if (existingRecord == null)
                            {
                                //daywise record save in tblShiftallocation
                                var shiftAllocation = new ShiftAllocation
                                {
                                    AllocationType = data.AllocationType,
                                    iCategory = Convert.ToInt32(data.SelectedCategoryId),
                                    iEmployee = emp,
                                    TimeFrame = data.TimeFrameType,
                                    iMonth = data.SelectedMonth ?? DateTime.Now.Month,
                                    iYear = data.SelectedYear ?? DateTime.Now.Year,
                                    iDay = i,
                                    dDate = new DateTime(data.SelectedYear ?? DateTime.Now.Year, data.SelectedMonth ?? DateTime.Now.Month, i),
                                    iShift = Convert.ToInt32(data.ShiftDays["Shift_Day_" + (i)]),
                                };
                                _db.shiftAllocations.Add(shiftAllocation);
                            }
                            else
                            {
                                //if record not exist then update the record
                                existingRecord.iShift = Convert.ToInt32(data.ShiftDays["Shift_Day_" + (i)]);
                                _db.shiftAllocations.Update(existingRecord);
                            }
                        }
                    }
                    _db.SaveChanges();
                }
                else if (data.TimeFrameType == "Yearly")
                {

                    //get no of days for selected month
                    int daysInYear = DateTime.IsLeapYear(data.SelectedYear ?? DateTime.Now.Year) ? 366 : 365;
                    for (int i = 1; i <= daysInYear; i++)
                    {
                        foreach (var emp in iEmployee)
                        {
                            //if combination of iemployee and ddate already exist in tblShiftallocation then update the record
                            ShiftAllocation? existingRecord = _db.shiftAllocations
                                .FirstOrDefault(x => x.iEmployee == emp && x.dDate == new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1));
                            int iMonth = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Month;
                            if (existingRecord == null)
                            {

                                //daywise record save in tblShiftallocation
                                var shiftAllocation = new ShiftAllocation
                                {
                                    AllocationType = data.AllocationType,
                                    iCategory = Convert.ToInt32(data.SelectedCategoryId),
                                    iEmployee = emp,
                                    TimeFrame = data.TimeFrameType,
                                    dDate = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1),
                                    iDay = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Day,
                                    iMonth = new DateTime(data.SelectedYear ?? DateTime.Now.Year, 1, 1).AddDays(i - 1).Month,
                                    iYear = data.SelectedYear ?? DateTime.Now.Year,
                                    iShift = data.ShiftMonths["Shift_Month_" + iMonth],
                                };
                                _db.shiftAllocations.Add(shiftAllocation);
                            }
                            else
                            {
                                //if record not exist then update the record
                                existingRecord.iShift = Convert.ToInt32(data.ShiftMonths["Shift_Month_" + (iMonth)]);
                                _db.shiftAllocations.Update(existingRecord);
                            }
                        }
                    }
                    _db.SaveChanges();
                }
                else
                {
                    return "Invalid time frame type.";
                }
                return "";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public IActionResult Success()
        {
            return View();
        }
    }
}
