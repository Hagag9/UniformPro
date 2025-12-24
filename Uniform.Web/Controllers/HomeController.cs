using Microsoft.AspNetCore.Mvc;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Controllers
{
    public class HomeController : FrontBaseController
    {
        public HomeController(ApplicationDbContext context) : base(context) { }

        public IActionResult Index()
        {
            ViewData["Title"] = "الرئيسية";
            return View();
        }
    }
}
