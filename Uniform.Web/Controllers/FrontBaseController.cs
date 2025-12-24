using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Controllers
{
    /// <summary>
    /// Base controller for all Frontend controllers.
    /// Injects SiteSettings into ViewData for Layout usage.
    /// </summary>
    public abstract class FrontBaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        protected FrontBaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Load SiteSettings once per request
            var settings = _context.SiteSettings.FirstOrDefault();

            if (settings != null)
            {
                ViewData["Phone"] = settings.PhoneNumber;
                ViewData["Phone2"] = settings.Phone2Number;
                ViewData["WhatsApp"] = settings.WhatsAppNumber;
                ViewData["Email"] = settings.Email;
                ViewData["Facebook"] = settings.FacebookUrl;
                ViewData["Instagram"] = settings.InstagramUrl;
                ViewData["TikTok"] = settings.TikTokUrl;
                ViewData["Youtube"] = settings.YoutubeUrl;
                ViewData["AddressAr"] = settings.AddressAr;
                ViewData["AddressEn"] = settings.AddressEn;
                ViewData["SiteNameAr"] = settings.WebsiteNameAr;
                ViewData["SiteNameEn"] = settings.WebsiteNameEn;
            }

            // Add culture info for Layout
            var currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            ViewData["IsArabic"] = currentCulture.StartsWith("ar");
            ViewData["CurrentCulture"] = currentCulture;
            ViewData["Dir"] = currentCulture.StartsWith("ar") ? "rtl" : "ltr";
            ViewData["Lang"] = currentCulture.StartsWith("ar") ? "ar" : "en";

            base.OnActionExecuting(context);
        }
    }
}
