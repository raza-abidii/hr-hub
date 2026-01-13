using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using EMSSolution.DataAccess;
using EMSSolution.GenericMethods;
using EMSSolution.LoggingService;
using EMSSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EMSSolution.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserActivityLogger? _userActivityLogger;
        private ApplicationDBContext _db;
        TableCreation tableCreation;
        string EncriptionKey = "SFONSoftwareSolutionLLPIndia2025";
        DatabaseAccessLayer.DataLayer dal = new DatabaseAccessLayer.DataLayer();
        string strQry = string.Empty;
        string strError = string.Empty;
        private readonly string _connectionString;
        List<SelectListItem> company = new List<SelectListItem>();
        public AccountController(IUserActivityLogger userActivityLogger, ApplicationDBContext db, IConfiguration configuration)
        {
            _userActivityLogger = userActivityLogger;
            _db = db;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult UserRights()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            List<SelectListItem> role = AppRoles.GetRoleSelectList();
            ViewBag.Roles = role;
            List<string> menuItems = MenuItem.getMenuItem();

            ViewBag.MenuItems = menuItems;
            return View();
        }

        public IActionResult UserCreation()
        {
            #region check if Session Live or Expired
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            List<SelectListItem> role = AppRoles.GetRoleSelectList();
            ViewBag.Roles = role;
            //get Employee
            ViewBag.Employee = _db.Employees.ToList();
            ViewBag.Users = _db.Users.ToList();
            ViewBag.BranchList=_db.Branches.ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UserCreation([FromBody] EMSUsers user)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                List<EMSUsers> lstUser = new List<EMSUsers>();
                //get next masterid for new entry
                if (user.Id != 0)
                {
                    var userExist = _db.Users.FirstOrDefault(c => c.Id != user.Id
                    && c.UserName == user.UserName);
                    if (userExist != null)
                    {
                        return this.Json(new { status = false, message = "User already exist", isNew = 0, data = "" });
                    }
                }
                else
                {
                    var userExist = _db.Users.FirstOrDefault(c => c.UserName == user.UserName);
                    if (userExist != null)
                    {
                        return this.Json(new { status = false, message = "User already exist", isNew = 0, data = "" });
                    }
                }
                if (user.Id == 0)
                {
                    iNew = 1;
                    strMessage = "Saved Successfully";
                    var pass = PasswordMasking.PasswordHasher.HashPassword(user.PasswordHash);
                    user.PasswordHash = pass.hashedPassword;
                    user.Salt = pass.salt;
                    _db.Users.Add(user);
                    lstUser.Add(user);
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                        , "Account", "UserCreation", "User : " + user.UserName + " created");
                }

                else

                {
                    iNew = 0;
                    strMessage = "Updated Successfully";
                    var usr = _db.Users.FirstOrDefault(c => c.Id == user.Id);
                    usr.UserName = user.UserName;
                    usr.Email = user.Email;
                    usr.Role = user.Role;
                    usr.iEmployee = user.iEmployee;
                    usr.iBranchList = user.iBranchList;
                    //usr.sImage = user.sImage;
                    _db.Users.Update(usr);
                    lstUser.Add(usr);

                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                       , "Account", "UserCreation", "User : " + user.UserName + " updated");
                }
                await _db.SaveChangesAsync();
                
                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = lstUser });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = iNew, data = "" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(int id)
        {
            int iNew = 1;
            string strMessage = "";
            try
            {
                EMSUsers? user = _db.Users.FirstOrDefault(c => c.Id == id);
                if (user != null)
                {
                    _db.Users.Remove(user);
                    await _db.SaveChangesAsync();
                    strMessage = "Deleted Successfully";
                    await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                        , HttpContext.Session.GetString("UserName")
                       , "Account", "DeleteUser", "User : " + user.UserName + " deleted");
                }
                else
                {
                    strMessage = "User not found";
                }
                return this.Json(new { status = true, message = strMessage, isNew = iNew, data = "" });
            }
            catch (Exception ex)
            {
                return this.Json(new { status = false, message = ex.Message, isNew = iNew, data = "" });
            }
        }
        public IActionResult Login()
        {
            ViewBag.HideSideNav = true;
            HttpContext.Session.SetString("HideSideNav", "true");
            GenericFunction.WriteLog("EMSSolution","Login");
            //initializeCompany();

            //ViewBag.companyList = company
            ViewBag.companyList = null;
            return View();
        }
        private void initializeCompany()
        {
            

            strQry = $@"select * from sys.databases where name='SFONEMS'";
            DataSet ds = dal.GetData(strQry, ref strError, "master");
            if (ds != null && ds.Tables[0].Rows.Count <= 0)
            {
                strQry = "Create Database SFONEMS";
                ds = dal.GetData(strQry, ref strError, "master");

                strQry = "Create Table tblCompany (companyName varchar(200),databaseName varchar(200),accountingDate datetime)";
                ds = dal.GetData(strQry, ref strError, "SFONEMS");
            }

            //strQry = $"select companyName + ' [' + databaseName + ']' company,databaseName from tblCompany";
            strQry = $"select companyName  company,databaseName from tblCompany";
            ds = dal.GetData(strQry, ref strError, "SFONEMS");
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    company.Add(new SelectListItem
                    {
                        Text = row["company"].ToString(),
                        Value = row["databaseName"].ToString()
                    });
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId")
                , HttpContext.Session.GetString("UserName")
                , "Home", "Logout", "User logged out");
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("ConnString", "");
            HttpContext.Session.SetString("database", "");

            
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //string dynamicConnString = _connectionString.Replace("Database=master", "Database=" + model.Company);
                    // Save it to session or user context
                    //HttpContext.Session.SetString("ConnString", dynamicConnString);
                    //HttpContext.Session.SetString("Database", model.Company);

                    var builder = new SqlConnectionStringBuilder(_connectionString);
                    string databaseName = builder.InitialCatalog;
                    HttpContext.Session.SetString("Database", databaseName);

                    //GenericFunction.WriteLog("EMSSolution", "Login Post Database: " + databaseName);

                    //GenericFunction.WriteLog("EMSSolution", "Table Creation Starts: " + databaseName);
                    tableCreation = new TableCreation(_db, databaseName);
                    tableCreation.CreateTable();
                    //GenericFunction.WriteLog("EMSSolution", "Table Creation Ends: " + databaseName);
                    

                    //var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
                    //optionsBuilder.UseSqlServer(dynamicConnString);
                    //_db = new ApplicationDBContext(optionsBuilder.Options);


                    // Here you can add your authentication logic
                    // For simplicity, we're just checking a hardcoded username and password
                    var user = _db.Users.FirstOrDefault(a => a.UserName == model.Username);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Invalid username or password");
                        TempData["LoginError"] = "Invalid username or password.";
                        ViewBag.LoginError = "Invalid username or password.";

                    }
                    else
                    {
                        string? storedHash = user.PasswordHash;
                        string? storedSalt = user.Salt;
                        bool bAuth = PasswordMasking.PasswordHasher.VerifyPassword(model.Password, storedHash, storedSalt);

                        //if (model.Username == "admin" && model.Password == "admin")
                        if (bAuth)
                        {

                            #region check for license
                            //string licenseKeyPath = System.IO.Path.Combine(AppContext.BaseDirectory, "license.key");
                            //dal.WriteLog("EMSSolution", "License Key Path: " + licenseKeyPath);
                            //if (!System.IO.File.Exists(licenseKeyPath))
                            //{
                            //    ViewBag.LoginError = "License Key Not found";
                            //    return View(model);
                            //}
                            //string strDecript = DecryptJson(licenseKeyPath, EncriptionKey);

                            //DecryptData decryptData = JsonConvert.DeserializeObject<DecryptData>(strDecript);
                            //string machineMacId = GetMacAddress();

                            //if (decryptData!=null)
                            //{
                            //    // License is valid, proceed with login
                            //    if (decryptData.Macid != machineMacId)
                            //    {
                            //        ViewBag.LoginError = "Invalid License Key";
                            //        return View(model);
                            //    }
                            //    if (decryptData.ExpiryDate.Date <= DateTime.Now.Date)
                            //    {
                            //        ViewBag.LoginError = "License Expired\nLicense expired Date:" + decryptData.ExpiryDate.Date.ToString("dd/MM/yyyy") + "\nPlease contact Administrator";
                            //        return View(model);
                            //    }
                            //}
                            //else
                            //{
                            //    ViewBag.LoginError = "Invalid License Key";
                            //    return View(model);
                            //}
                            #endregion

                            // Redirect to a different page after successful login
                            //return RedirectToAction("Index", "Home");
                            //HttpContext.Session.SetString("IsAuthenticated", "true");

                            //tableCreation = new TableCreation(_db, model.Company);
                            //tableCreation = new TableCreation(_db, databaseName);

                            //GenericFunction.WriteLog("EMSSolution", "Table Creation: " + databaseName);
                            //tableCreation.CreateTable();

                            HttpContext.Session.SetString("IsAuthenticated", "true");

                            var properCaseUsername = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.Username.ToLower());
                            HttpContext.Session.SetString("UserName", properCaseUsername);

                            var userId = _db.Users.Where(a => a.UserName == model.Username)
                                .Select(a => new { a.Id, a.Role, a.iEmployee, a.iBranchList })
                                .FirstOrDefault();
                            HttpContext.Session.SetString("UserId", userId.Id.ToString());
                            HttpContext.Session.SetString("Role", userId.Role.ToString());

                            HttpContext.Session.SetString("EmployeeId", userId.iEmployee.ToString());

                            HttpContext.Session.SetString("BranchList", userId.iBranchList.ToString());


                            //this is to Show Leave Alert for Second level leave approval process with HR
                            Preference SecondLevel = _db.Preferences.FirstOrDefault();
                            if (SecondLevel != null && Convert.ToInt32(SecondLevel.secLvlLeaveAppUser) == userId.Id)
                            {
                                HttpContext.Session.SetString("HRRole", "1");
                            }
                            else
                                HttpContext.Session.SetString("HRRole", "0");

                            ViewBag.HideSideNav = false;
                            HttpContext.Session.SetString("HideSideNav", "false");

                            //Get All Rights and set in session
                            List<string> lstRights = new List<string>();
                            var userRigths = (from ur in _db.userRights
                                              join usr in _db.Users on ur.Role equals usr.Role
                                              where usr.UserName == model.Username
                                              select ur.Menuitem).ToList();
                            string strUserRights = string.Join(",", userRigths);

                            HttpContext.Session.SetString("UserRights", strUserRights);


                            HttpContext.Session.SetString("AppTheme", "theme-dark.css");


                            var company = _db.companies.FirstOrDefault();
                            if (company != null && company.Logo != null)
                            {
                                //string base64Logo = company.Logo;
                                //ViewBag.CompanyLogo = base64Logo;
                                //ViewBag.CompanyName = company.CompanyName;

                                HttpContext.Session.SetString("CompanyLogo", company.Logo);
                                HttpContext.Session.SetString("CompanyName", company.CompanyName);
                            }

                            await _userActivityLogger.LogAsync(HttpContext.Session.GetString("UserId"),
                                model.Username, "Account", "Login", "User logged in");
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password");
                            TempData["LoginError"] = "Invalid username or password.";
                            ViewBag.LoginError = "Invalid username or password.";
                        }

                    }
                    initializeCompany();
                    ViewBag.companyList = company;
                }

                // If we get here, something failed, so we re-render the login page
                return View(model);
            }
            catch (Exception e1)
            {
                GenericFunction.WriteLog("EMSSolution", "Login: Exception: " + e1);
                return this.Json(new { status = false, message = e1.Message, data = "" });
            }
        }

        public static string GetMacAddress()
        {
            var nic = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.GetPhysicalAddress().GetAddressBytes().Length == 6);
            if (nic != null)
            {
                var bytes = nic.GetPhysicalAddress().GetAddressBytes();
                return string.Join("-", bytes.Select(b => b.ToString("X2")));
            }
            return "MAC Address Not Found";
        }

        // AES Decryption
        static string DecryptJson(string lisenceKeyPath, string key)
        {
            try
            {
                string encryptedJson= System.IO.File.ReadAllText(lisenceKeyPath);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key.Substring(0, 16)); // AES key must be 16 bytes
                    aesAlg.IV = new byte[16]; // Initial vector set to 0 (should match the encryption IV)

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedJson)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle decryption errors
                Console.WriteLine($"Decryption failed: {ex.Message}");
                return $"Decryption Exception: {ex.Message}";
            }
        }

    }
}
