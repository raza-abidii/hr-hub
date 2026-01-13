using Azure;
using ClosedXML.Excel;
using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EMSSolution.Controllers
{
    public class EmployeeMasterReportController : Controller
    {
        readonly ApplicationDBContext _db;
        string database = string.Empty;

        public IActionResult Index()
        {
            return View();
        }
        public EmployeeMasterReportController(ApplicationDBContext db)
        {
            _db = db;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            database = HttpContext.Session.GetString("Database");
        }

        [HttpGet]
        public IActionResult GetEmployeesReport(int employeeId, int branchId)
        {
            try
            {
                var query =
                    from e in _db.Employees
                    join b in _db.Branches on e.iBranch equals b.iMasterid into empBranch
                    from branch in empBranch.DefaultIfEmpty()
                    join c in _db.Categories on e.iCategory equals c.iMasterid into empCategory
                    from category in empCategory.DefaultIfEmpty()
                    join d in _db.Departments on e.iDepartment equals d.iMasterid into empDept
                    from dept in empDept.DefaultIfEmpty()
                    join desi in _db.Designations on e.iDesignation equals desi.iMasterid into empDesi
                    from designation in empDesi.DefaultIfEmpty()
                    where e.bEmployeeResign==false 
                    orderby (branch != null ? branch.sName : ""), e.sEmployeeName
                    select new EmployeeMasterReport
                    {
                        sEmployeeName = e.sEmployeeName,
                        sEmployeeCode = e.sEmployeeCode,
                        iEmployeeId = e.iMasterid,
                        Branch = branch != null ? branch.sName : "",
                        iBranchId = branch != null ? branch.iMasterid : 0,
                        Category = category != null ? category.sName : "",
                        Department = dept != null ? dept.sName : "",
                        Designation = designation != null ? designation.sName : "",
                        MobileNo = e.sPhoneNo ?? "",
                        EmailId = e.sEmailId ?? ""
                    };

                List<EmployeeMasterReport> employeeList = query.ToList();
                if (employeeId != 0)
                {
                    employeeList = employeeList.Where(e => e.iEmployeeId == employeeId).ToList();
                }
                if (branchId != 0)
                {
                    employeeList = employeeList.Where(e => e.iBranchId == branchId).ToList();
                }
                string strBranchList = HttpContext.Session.GetString("BranchList") ?? "";
                if (!string.IsNullOrEmpty(strBranchList))
                {
                    employeeList = employeeList.Where(e => strBranchList.Split(',').Contains(e.iBranchId.ToString())).ToList();
                }

                return Json(new { success = true, message = "", data = employeeList });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = "" });
            }
        }

        [HttpPost]
        public ActionResult ExportToExcel([FromBody] TableDataModel model)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Employees");

                // Add Company Name (Row 1)
                string companyName= HttpContext.Session.GetString("CompanyName");
                ws.Cell(1, 1).Value = companyName;  // replace with your variable
                ws.Range(1, 1, 1, model.TableData[0].Count).Merge(); // Merge across all columns
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Add Report Title (Row 2)
                ws.Cell(2, 1).Value = "Employee Master Report"; // replace with your title
                ws.Range(2, 1, 2, model.TableData[0].Count).Merge();
                ws.Cell(2, 1).Style.Font.Bold = true;
                ws.Cell(2, 1).Style.Font.FontSize = 14;
                ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                for (int i = 0; i < model.TableData.Count; i++)
                {
                    for (int j = 0; j < model.TableData[i].Count; j++)
                    {
                        ws.Cell(i + 3, j + 1).Value = model.TableData[i][j];

                        // Apply style to header row
                        if (i == 0)
                        {
                            ws.Cell(i + 3, j + 1).Style.Font.Bold = true;
                            ws.Cell(i + 3, j + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }
                    }
                }
                ws.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeMasterReport.xlsx");

                }
            }
        }
    }
}
public class TableDataModel
{
    public List<List<string>> TableData { get; set; }
}
