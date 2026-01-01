using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Helpers;
using UniformPro.Web.Services;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public SettingsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
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
        public async Task<IActionResult> Update(SiteSettings model, IFormFile? ownerImageFile)
        {
            if (ModelState.IsValid)
            {
                // معالجة رفع صورة المالك
                if (ownerImageFile != null && ownerImageFile.Length > 0)
                {
                    try
                    {
                        // حذف الصورة القديمة إن وجدت
                        if (!string.IsNullOrEmpty(model.OwnerImage))
                        {
                            _fileService.DeleteFile(model.OwnerImage, Constants.Folders.SiteSettings);
                        }

                        // رفع الصورة الجديدة
                        model.OwnerImage = await _fileService.SaveFileAsync(ownerImageFile, Constants.Folders.SiteSettings);
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("OwnerImage", ex.Message);
                        return View("Index", model);
                    }
                }

                _context.SiteSettings.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث إعدادات الموقع بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", model);
        }
    }
}