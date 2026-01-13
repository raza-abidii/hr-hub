using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.CodeDom;

namespace EMSSolution.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDBContext _db;
        public TestController(ApplicationDBContext dBContext)
        {
            _db = dBContext;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult TestPage()
        {
            string[] strings = { "One","Two","Three" };
            string str = "DALDA";
            string strResult=string.Concat(str.Reverse());
            return View();
        }
    }
}
