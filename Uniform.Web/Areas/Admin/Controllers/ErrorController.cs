using Microsoft.AspNetCore.Mvc;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorController : Controller
    {
        // خطأ 404: الصفحة غير موجودة
        [Route("Admin/Error/NotFound")]
        public IActionResult NotFound(int code)
        {
            return View();
        }

        // خطأ 500: مشكلة في السيرفر
        [Route("Admin/Error/General")]
        public IActionResult General()
        {
            // لو الطلب AJAX رجع JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "حدث خطأ غير متوقع في النظام." });
            }

            return View();
        }
    }
}