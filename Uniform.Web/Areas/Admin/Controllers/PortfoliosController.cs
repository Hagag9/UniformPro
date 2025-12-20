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
    [Authorize]
    public class PortfoliosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public PortfoliosController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
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
                    DescriptionAr = model.DescriptionAr,
                    DescriptionEn = model.DescriptionEn,
                    CategoryId = model.CategoryId,
                    ShowOnHome = model.ShowOnHome,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    CreatedAt = DateTime.Now
                };

                if (model.CoverImageFile != null)
                {
                    portfolio.CoverImagePath = await _fileService.SaveFileAsync(model.CoverImageFile, "portfolios/covers");
                }

                _context.Add(portfolio);
                await _context.SaveChangesAsync();

                // رفع الميديا (صور وفيديو)
                if (model.MediaFiles != null)
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
        public async Task<IActionResult> Edit(int id, PortfolioViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.Id == id);
                if (portfolio == null) return NotFound();

                portfolio.ClientNameAr = model.ClientNameAr;
                portfolio.ClientNameEn = model.ClientNameEn;
                portfolio.DescriptionAr = model.DescriptionAr;
                portfolio.DescriptionEn = model.DescriptionEn;
                portfolio.CategoryId = model.CategoryId;
                portfolio.ShowOnHome = model.ShowOnHome;
                portfolio.MetaDescription = model.MetaDescription;
                portfolio.MetaKeywords = model.MetaKeywords;

                if (model.CoverImageFile != null)
                {
                    if (!string.IsNullOrEmpty(portfolio.CoverImagePath))
                        _fileService.DeleteFile(portfolio.CoverImagePath, "portfolios/covers");

                    portfolio.CoverImagePath = await _fileService.SaveFileAsync(model.CoverImageFile, "portfolios/covers");
                }

                if (model.MediaFiles != null)
                {
                    foreach (var file in model.MediaFiles)
                    {
                        var path = await _fileService.SaveFileAsync(file, "portfolios/media");
                        var isVideo = file.ContentType.ToLower().StartsWith("video");
                        _context.PortfolioMedias.Add(new PortfolioMedia { PortfolioId = portfolio.Id, MediaUrl = path, Type = isVideo ? MediaType.Video : MediaType.Image });
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

        // ================== DELETE MEDIA (AJAX) ==================
        [HttpPost]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var media = await _context.PortfolioMedias.FindAsync(id);
            if (media != null)
            {
                // نحذف الملف فقط إذا لم يكن رابط يوتيوب (لا يبدأ بـ http)
                if (!media.MediaUrl.StartsWith("http"))
                {
                    _fileService.DeleteFile(media.MediaUrl, "portfolios/media");
                }

                _context.PortfolioMedias.Remove(media);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        // ================== DELETE PORTFOLIO ==================
        public async Task<IActionResult> Delete(int id)
        {
            var portfolio = await _context.Portfolios
                                          .Include(p => p.PortfolioMedias)
                                          .FirstOrDefaultAsync(p => p.Id == id);
            if (portfolio != null)
            {
                // حذف الغلاف
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