using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // صفحة التعديل (هي الصفحة الرئيسية هنا)
        public IActionResult Index()
        {
            // نجلب الصف الأول والوحيد
            var setting = _context.SiteSettings.FirstOrDefault();
            if (setting == null) return NotFound();

            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SiteSettings model)
        {
            if (ModelState.IsValid)
            {
                _context.SiteSettings.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث إعدادات الموقع بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", model);
        }
    }
}