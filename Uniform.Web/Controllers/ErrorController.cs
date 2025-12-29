using Microsoft.AspNetCore.Mvc;

namespace UniformPro.Web.Controllers
{
    public class ErrorController : Controller
    {
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
