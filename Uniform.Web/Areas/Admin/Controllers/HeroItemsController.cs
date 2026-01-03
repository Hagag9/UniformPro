using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;
using Microsoft.Extensions.Logging;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class HeroItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<HeroItemsController> _logger;

        public HeroItemsController(ApplicationDbContext context, IFileService fileService, ILogger<HeroItemsController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.HeroItems.OrderBy(h => h.DisplayOrder).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HeroItem heroItem, IFormFile? desktopImage, IFormFile? mobileImage)
        {
            try
            {
                ModelState.Remove("ImagePath");
                if (desktopImage == null)
                {
                    ModelState.AddModelError("ImagePath", "يرجى رفع صورة الديسكتوب");
                }

                if (ModelState.IsValid && desktopImage != null)
                {
                    try
                    {
                        // 1. Save Desktop
                        heroItem.ImagePath = await _fileService.SaveHeroDesktopImageAsync(desktopImage);
                    
                        // 2. Save Mobile (or generate from desktop if missing)
                        if (mobileImage != null)
                        {
                            heroItem.MobileImagePath = await _fileService.SaveHeroMobileImageAsync(mobileImage);
                        }
                        else
                        {
                            // Fallback: Generate mobile version from desktop file
                            heroItem.MobileImagePath = await _fileService.SaveHeroMobileImageAsync(desktopImage);
                        }

                        _context.Add(heroItem);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Error processing images in Create HeroItem");
                        ModelState.AddModelError("ImagePath", ex.Message);
                        return View(heroItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Create));
                return Redirect("/Admin/Error/General");
            }
            return View(heroItem);
        }

        // الحذف
        // الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.HeroItems.FindAsync(id);
                if (item != null)
                {
                    // حذف الصورتين (Desktop + Mobile)
                    _fileService.DeleteFile(item.ImagePath, "hero");
                    if (!string.IsNullOrEmpty(item.MobileImagePath))
                    {
                        _fileService.DeleteFile(item.MobileImagePath, "hero");
                    }
                    
                    _context.HeroItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Delete));
                return Redirect("/Admin/Error/General");
            }
            return RedirectToAction(nameof(Index));
        }

        // التعديل
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.HeroItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HeroItem heroItem, IFormFile? desktopImage, IFormFile? mobileImage)
        {
            if (id != heroItem.Id)
            {
                return NotFound();
            }

            ModelState.Remove("ImagePath");

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        // 1. Handle Desktop Image Update
                        if (desktopImage != null)
                        {
                            // Delete old desktop
                            _fileService.DeleteFile(heroItem.ImagePath, "hero");
                            // Save new desktop
                            heroItem.ImagePath = await _fileService.SaveHeroDesktopImageAsync(desktopImage);

                            // If mobile NOT provided, regenerate mobile from new desktop (standard behavior)
                            if (mobileImage == null)
                            {
                                if (!string.IsNullOrEmpty(heroItem.MobileImagePath)) _fileService.DeleteFile(heroItem.MobileImagePath, "hero");
                                heroItem.MobileImagePath = await _fileService.SaveHeroMobileImageAsync(desktopImage);
                            }
                        }

                        // 2. Handle Mobile Image Update (Explicitly provided)
                        if (mobileImage != null)
                        {
                            // Delete old mobile
                            if (!string.IsNullOrEmpty(heroItem.MobileImagePath)) _fileService.DeleteFile(heroItem.MobileImagePath, "hero");
                            // Save new mobile
                            heroItem.MobileImagePath = await _fileService.SaveHeroMobileImageAsync(mobileImage);
                        }

                        _context.Update(heroItem);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "تم تحديث الشريحة بنجاح";
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Error processing images in Edit HeroItem");
                        ModelState.AddModelError("", ex.Message);
                        return View(heroItem);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!await _context.HeroItems.AnyAsync(e => e.Id == id))
                        {
                            return NotFound();
                        }
                        _logger.LogError(ex, "Concurrency error in Edit HeroItem");
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Edit));
                return Redirect("/Admin/Error/General");
            }
            return View(heroItem);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> sortedIds)
        {
            try
            {
                // 1. جلب العناصر الموجودة
                var items = await _context.HeroItems.ToListAsync();

                // 2. تحديث الترتيب بناءً على الليستة القادمة من الفرونت
                int order = 1;
                foreach (var id in sortedIds)
                {
                    var item = items.FirstOrDefault(x => x.Id == id);
                    if (item != null)
                    {
                        item.DisplayOrder = order++;
                        _context.Update(item);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(UpdateOrder));
                return BadRequest();
            }
        }
    }
}