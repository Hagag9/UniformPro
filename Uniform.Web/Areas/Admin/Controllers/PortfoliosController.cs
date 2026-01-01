using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Helpers;
using UniformPro.Web.Services;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class PortfoliosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IHtmlSanitizerService _sanitizer;

        public PortfoliosController(ApplicationDbContext context, IFileService fileService, IHtmlSanitizerService sanitizer)
        {
            _context = context;
            _fileService = fileService;
            _sanitizer = sanitizer;
        }

        // عرض القائمة
    public async Task<IActionResult> Index(string searchString, int? pageNumber)
        
        {
        ViewData["CurrentFilter"] = searchString;

        var items = _context.Portfolios.Include(p => p.Category).AsQueryable();

        // 1. منطق البحث
        if (!string.IsNullOrEmpty(searchString))
        {
            items = items.Where(p => p.ClientNameAr.Contains(searchString)
                                  || p.ClientNameEn.Contains(searchString)
                                  || p.Category.NameAr.Contains(searchString));
        }

        // الترتيب: الأحدث أولاً
        items = items.OrderByDescending(p => p.CreatedAt);

        // 2. التقسيم (9 عناصر في الصفحة عشان الشكل يكون 3x3)
        int pageSize = 9;
        var paginatedList = await PaginatedList<Portfolio>.CreateAsync(items.AsNoTracking(), pageNumber ?? 1, pageSize);

        // 3. لو AJAX رجع الـ Partial بس
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_PortfolioList", paginatedList);
        }

        return View(paginatedList);
    }

        // ================== CREATE ==================
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr");
            return View(new PortfolioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortfolioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var portfolio = new Portfolio
                {
                    ClientNameAr = model.ClientNameAr,
                    ClientNameEn = model.ClientNameEn,
                    DescriptionAr = _sanitizer.Sanitize(model.DescriptionAr),
                    DescriptionEn = _sanitizer.Sanitize(model.DescriptionEn),
                    CategoryId = model.CategoryId,
                    ShowOnHome = model.ShowOnHome,
                    DisplayOrder = model.DisplayOrder,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    CreatedAt = DateTime.Now
                };

                if (model.CoverImageFile != null)
                {
                    try
                    {
                        portfolio.CoverImagePath = await _fileService.SaveFileAsync(model.CoverImageFile, "portfolios/covers");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("CoverImageFile", ex.Message);
                        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
                        return View(model);
                    }
                }

                _context.Add(portfolio);
                await _context.SaveChangesAsync();

                // رفع الميديا (صور وفيديو)
                if (model.MediaFiles != null)
                {
                    try 
                    {
                        foreach (var file in model.MediaFiles)
                        {
                            var path = await _fileService.SaveFileAsync(file, "portfolios/media");
                            var isVideo = file.ContentType.ToLower().StartsWith("video");
                            _context.PortfolioMedias.Add(new PortfolioMedia
                            {
                                PortfolioId = portfolio.Id,
                                MediaUrl = path,
                                Type = isVideo ? MediaType.Video : MediaType.Image
                            });
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        TempData["ErrorMessage"] = $"تم حفظ المشروع ولكن فشل رفع بعض الوسائط: {ex.Message}";
                    }
                }

                // حفظ روابط يوتيوب
                if (model.YoutubeUrls != null)
                {
                    foreach (var url in model.YoutubeUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                    {
                        _context.PortfolioMedias.Add(new PortfolioMedia { PortfolioId = portfolio.Id, MediaUrl = url, Type = MediaType.Video });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة المشروع بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios.Include(p => p.PortfolioMedias).FirstOrDefaultAsync(p => p.Id == id);
            if (portfolio == null) return NotFound();

            var model = new PortfolioViewModel
            {
                Id = portfolio.Id,
                ClientNameAr = portfolio.ClientNameAr,
                ClientNameEn = portfolio.ClientNameEn,
                DescriptionAr = portfolio.DescriptionAr,
                DescriptionEn = portfolio.DescriptionEn,
                CategoryId = portfolio.CategoryId,
                ShowOnHome = portfolio.ShowOnHome,
                DisplayOrder = portfolio.DisplayOrder,
                CurrentCoverImagePath = portfolio.CoverImagePath,
                CurrentMedia = portfolio.PortfolioMedias.ToList(),
                MetaDescription = portfolio.MetaDescription,
                MetaKeywords = portfolio.MetaKeywords
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PortfolioViewModel model, string? deletedMediaIds, bool deleteCoverImage = false)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var portfolio = await _context.Portfolios.Include(p => p.PortfolioMedias).FirstOrDefaultAsync(p => p.Id == id);
                if (portfolio == null) return NotFound();

                // 1. معالجة الميديا المحذوفة (Deferred Deletion)
                if (!string.IsNullOrEmpty(deletedMediaIds))
                {
                    var idsToDelete = deletedMediaIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(i => int.Parse(i)).ToList();

                    var itemsToDelete = portfolio.PortfolioMedias.Where(m => idsToDelete.Contains(m.Id)).ToList();
                    
                    foreach (var item in itemsToDelete)
                    {
                        // حذف الملف الفعلي فقط إذا لم يكن يوتيوب
                         if (!item.MediaUrl.StartsWith("http"))
                        {
                            _fileService.DeleteFile(item.MediaUrl, "portfolios/media");
                        }
                        // حذف من الداتابيز
                        _context.PortfolioMedias.Remove(item);
                    }
                }

                portfolio.ClientNameAr = model.ClientNameAr;
                portfolio.ClientNameEn = model.ClientNameEn;
                portfolio.DescriptionAr = _sanitizer.Sanitize(model.DescriptionAr);
                portfolio.DescriptionEn = _sanitizer.Sanitize(model.DescriptionEn);
                portfolio.CategoryId = model.CategoryId;
                portfolio.ShowOnHome = model.ShowOnHome;
                portfolio.DisplayOrder = model.DisplayOrder;
                portfolio.MetaDescription = model.MetaDescription;
                portfolio.MetaKeywords = model.MetaKeywords;

                // 2. معالجة حذف صورة الغلاف (Deferred)
                if (deleteCoverImage && !string.IsNullOrEmpty(portfolio.CoverImagePath))
                {
                    _fileService.DeleteFile(portfolio.CoverImagePath, "portfolios/covers");
                    portfolio.CoverImagePath = null;
                }

                if (model.CoverImageFile != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(portfolio.CoverImagePath))
                            _fileService.DeleteFile(portfolio.CoverImagePath, "portfolios/covers");

                        portfolio.CoverImagePath = await _fileService.SaveFileAsync(model.CoverImageFile, "portfolios/covers");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("CoverImageFile", ex.Message);
                        // Reload data
                        var existingP = await _context.Portfolios.Include(p => p.PortfolioMedias).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (existingP != null)
                        {
                            model.CurrentCoverImagePath = existingP.CoverImagePath;
                            model.CurrentMedia = existingP.PortfolioMedias.ToList();
                        }
                        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
                        return View(model);
                    }
                }

                if (model.MediaFiles != null)
                {
                    try
                    {
                        foreach (var file in model.MediaFiles)
                        {
                            var path = await _fileService.SaveFileAsync(file, "portfolios/media");
                            var isVideo = file.ContentType.ToLower().StartsWith("video");
                            _context.PortfolioMedias.Add(new PortfolioMedia { PortfolioId = portfolio.Id, MediaUrl = path, Type = isVideo ? MediaType.Video : MediaType.Image });
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        TempData["ErrorMessage"] = $"تم تحديث المشروع ولكن فشل رفع بعض الوسائط: {ex.Message}";
                    }
                }

                if (model.YoutubeUrls != null)
                {
                    foreach (var url in model.YoutubeUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                    {
                        _context.PortfolioMedias.Add(new PortfolioMedia { PortfolioId = portfolio.Id, MediaUrl = url, Type = MediaType.Video });
                    }
                }

                _context.Update(portfolio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم التحديث بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _context.Portfolios.Include(p => p.PortfolioMedias).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existing != null)
            {
                model.CurrentCoverImagePath = existing.CoverImagePath;
                model.CurrentMedia = existing.PortfolioMedias.ToList();
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", model.CategoryId);
            return View(model);
        }

        // ================== DELETE PORTFOLIO ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var portfolio = await _context.Portfolios
                                          .Include(p => p.PortfolioMedias)
                                          .FirstOrDefaultAsync(p => p.Id == id);
            if (portfolio != null)
            {
                // 1. حذف التقييمات المرتبطة (Cascade Delete Manual)
                var relatedTestimonials = await _context.Testimonials.Where(t => t.PortfolioId == id).ToListAsync();
                foreach (var testimonial in relatedTestimonials)
                {
                    if (!string.IsNullOrEmpty(testimonial.ImagePath))
                        _fileService.DeleteFile(testimonial.ImagePath, "testimonials/images");

                    if (!string.IsNullOrEmpty(testimonial.VideoPath))
                        _fileService.DeleteFile(testimonial.VideoPath, "testimonials/videos");
                        
                    if (!string.IsNullOrEmpty(testimonial.CoverImage))
                        _fileService.DeleteFile(testimonial.CoverImage, "testimonials/covers"); // Assuming folder structure

                    _context.Testimonials.Remove(testimonial);
                }

                // 2. حذف الغلاف
                if (!string.IsNullOrEmpty(portfolio.CoverImagePath))
                    _fileService.DeleteFile(portfolio.CoverImagePath, "portfolios/covers");

                // حذف الميديا
                foreach (var media in portfolio.PortfolioMedias)
                {
                    if (!media.MediaUrl.StartsWith("http"))
                        _fileService.DeleteFile(media.MediaUrl, "portfolios/media");
                }

                _context.Portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}