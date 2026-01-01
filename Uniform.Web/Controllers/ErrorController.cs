using Microsoft.AspNetCore.Mvc;

namespace UniformPro.Web.Controllers
{
    public class ErrorController : FrontBaseController
    {
        public ErrorController(UniformPro.Infrastructure.Data.ApplicationDbContext context) : base(context)
        {
        }
        [Route("Error/NotFound")]
        public new IActionResult NotFound()
        {
            return View();
        }

        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
            return View();
        }
    }
}
