using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Helpers;
using UniformPro.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.OutputCaching; // Added namespace
using Microsoft.EntityFrameworkCore; // Added namespace

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<SettingsController> _logger;
        private readonly IOutputCacheStore _cacheStore; // Added Cache Store

        public SettingsController(ApplicationDbContext context, IFileService fileService, ILogger<SettingsController> logger, IOutputCacheStore cacheStore)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _cacheStore = cacheStore; // Assign Cache Store
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
        public async Task<IActionResult> Update(SiteSettings model, IFormFile? ownerImageFile, IFormFile? logoFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // تنظيف رابط الخريطة: إذا قام المستخدم بلصق كود Iframe كامل، نستخرج الرابط فقط
                    if (!string.IsNullOrWhiteSpace(model.MapLocationUrl) && model.MapLocationUrl.Contains("<iframe"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(model.MapLocationUrl, "src=\"([^\"]+)\"");
                        if (match.Success)
                        {
                            model.MapLocationUrl = match.Groups[1].Value;
                        }
                    }
                    // معالجة رفع صورة المالك
                    if (ownerImageFile != null && ownerImageFile.Length > 0)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(model.OwnerImage))
                            {
                                _fileService.DeleteFile(model.OwnerImage, Constants.Folders.SiteSettings);
                            }
                            model.OwnerImage = await _fileService.SaveFileAsync(ownerImageFile, Constants.Folders.SiteSettings);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading owner image in Settings");
                            ModelState.AddModelError("OwnerImage", ex.Message);
                            return View("Index", model);
                        }
                    }

                    // معالجة رفع اللوجو
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(model.LogoPath))
                            {
                                _fileService.DeleteFile(model.LogoPath, Constants.Folders.SiteSettings);
                            }
                            model.LogoPath = await _fileService.SaveFileAsync(logoFile, Constants.Folders.SiteSettings);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading logo in Settings");
                            ModelState.AddModelError("LogoPath", ex.Message);
                            return View("Index", model);
                        }
                    }

                    _context.SiteSettings.Update(model);
                    await _context.SaveChangesAsync();

                    // Evict Home Cache because settings (Logo, Phones) are on Home
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);

                    TempData["SuccessMessage"] = "تم تحديث إعدادات الموقع بنجاح!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Update));
                return Redirect("/Admin/Error/General");
            }
            return View("Index", model);
        }
    }
}