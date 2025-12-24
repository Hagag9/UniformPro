using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace UniformPro.Web.Controllers
{
    /// <summary>
    /// Controller for handling language switching.
    /// </summary>
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult Switch(string culture, string returnUrl = "/")
        {
            // Validate culture
            if (culture != "ar" && culture != "en")
            {
                culture = "ar"; // Default to Arabic
            }

            // Set the culture cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // Redirect back to the previous page
            return LocalRedirect(returnUrl);
        }
    }
}
