using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;

namespace EMSSolution.Controllers
{
    public class CompanyController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDBContext _db;
        string database = string.Empty;
        string strQry= string.Empty,ErrMessage=string.Empty;
        DatabaseAccessLayer.DataLayer dal = new DatabaseAccessLayer.DataLayer();
        public CompanyController(IWebHostEnvironment env,ApplicationDBContext db)
        {
            _env = env;
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

        [HttpGet]
        //get company
        public IActionResult GetCompany()
        {
            try
            {
                var company = _db.companies.FirstOrDefault();
                if (company == null)
                    return this.Json(new { status = false, message = "Company Data not found" });
                else
                    return this.Json(new { status = true, message = "", data = company });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = "Exception: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCompany([FromBody]  Company companyDetail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CompanyForm", companyDetail);
                }

               
                var company = _db.companies.ToList();
                _db.companies.RemoveRange(company);
               
                _db.SaveChanges();
                _db.companies.Add(companyDetail);
                _db.SaveChanges();

                return this.Json(new { status = true, message = "Company Saved Sucessfully"});

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the company details: " + ex.Message);
                return this.Json(new { status = false, message = ex.Message});
            }

        }

        [HttpPost]
        public IActionResult createCompany(string companyName,string databaseName, DateTime accountingDate)
        {
            try
            {
                strQry = $@"select * from sys.databases where name='{databaseName}'";
                DataSet ds=dal.GetData(strQry, ref ErrMessage);
                if(ds.Tables[0].Rows.Count > 0)
                {
                    return this.Json(new { status = false, message = "Database with this name already exists. Please choose a different name." });
                }
                strQry = $@"select * from sys.databases where name='SFONEMS'";
                ds = dal.GetData(strQry, ref ErrMessage);
                if (ds.Tables[0].Rows.Count <= 0)
                {
                    return this.Json(new { status = false, message = "SFONEMS Database does not exist, please check with administrator" });
                }
                strQry = $@"Create Database {databaseName}";
                dal.GetExecute(strQry, ref ErrMessage);

                strQry = $@"Insert into SFONEMS..tblCompany values('{companyName}','{databaseName}','{accountingDate.ToString("yyyy-MM-dd")}')";
                dal.GetExecute(strQry, ref ErrMessage);

                TableCreation tableCreation = new TableCreation(_db, databaseName);
                tableCreation.CreateTable();

                return this.Json(new { status = true, message = "Company Created Sucessfully" });
            }
            catch (Exception ex)
            {
                
                return this.Json(new { status = false, message = "Exception in createCompany: " + ex.Message });
            }
        }
    }
}
