using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;
using Microsoft.AspNetCore.OutputCaching; // Added
using UniformPro.Web.ViewModels;
using Microsoft.Extensions.Logging;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<TestimonialsController> _logger;
        private readonly IOutputCacheStore _cacheStore; // Added

        public TestimonialsController(ApplicationDbContext context, IFileService fileService, ILogger<TestimonialsController> logger, IOutputCacheStore cacheStore)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _cacheStore = cacheStore;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Testimonials.OrderByDescending(t => t.CreatedAt).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr");
            return View(new TestimonialViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestimonialViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // التحقق: يجب وجود فيديو مرفوع أو رابط يوتيوب -> تم الايقاف لان الحقول Nullable
                    /*
                    if (model.VideoFile == null && string.IsNullOrEmpty(model.YoutubeUrl))
                    {
                        ModelState.AddModelError("", "يجب إضافة فيديو (رفع ملف) أو رابط يوتيوب");
                        ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                        return View(model);
                    }
                    */

                    var testimonial = new Testimonial
                    {
                        ClientName = model.ClientName,
                        Position = model.Position,
                        Feedback = model.Feedback,
                        YoutubeUrl = model.YoutubeUrl,
                        IsActive = model.IsActive,
                        ShowOnHome = model.ShowOnHome, // ✅ اضافة
                        ProductNameAr = model.ProductNameAr,
                        ProductNameEn = model.ProductNameEn,
                        CreatedAt = DateTime.Now,
                        PortfolioId = model.PortfolioId
                    };

                    // حفظ صورة الفديو (Cover Image)
                    if (model.CoverImageFile != null)
                    {
                        try
                        {
                            testimonial.CoverImage = await _fileService.SaveFileAsync(model.CoverImageFile, "testimonials/covers");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading cover image in Create Testimonial");
                            ModelState.AddModelError("CoverImageFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    // حفظ الصورة
                    if (model.ImageFile != null)
                    {
                        try
                        {
                            testimonial.ImagePath = await _fileService.SaveFileAsync(model.ImageFile, "testimonials/images");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading image in Create Testimonial");
                            ModelState.AddModelError("ImageFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    // حفظ الفيديو
                    if (model.VideoFile != null)
                    {
                        try
                        {
                            testimonial.VideoPath = await _fileService.SaveFileAsync(model.VideoFile, "testimonials/videos");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading video in Create Testimonial");
                            ModelState.AddModelError("VideoFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    _context.Add(testimonial);
                    await _context.SaveChangesAsync();

                    // Evict Home Cache
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);

                    TempData["SuccessMessage"] = "تم إضافة التقييم بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Create));
                return Redirect("/Admin/Error/General");
            }
            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Testimonials.FindAsync(id);
            if (item == null) return NotFound();

            var model = new TestimonialViewModel
            {
                Id = item.Id,
                ClientName = item.ClientName,
                Position = item.Position,
                Feedback = item.Feedback,
                YoutubeUrl = item.YoutubeUrl,
                IsActive = item.IsActive,
                ShowOnHome = item.ShowOnHome, // ✅ اضافة
                ProductNameAr = item.ProductNameAr,
                ProductNameEn = item.ProductNameEn,
                CurrentImagePath = item.ImagePath,
                CurrentCoverImage = item.CoverImage, // ✅ اضافة
                CurrentVideoPath = item.VideoPath,
                PortfolioId = item.PortfolioId
            };

            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", item.PortfolioId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestimonialViewModel model, bool deleteClientImage = false, bool deleteClientVideo = false, bool deleteClientCoverImage = false)
        {
            if (id != model.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var item = await _context.Testimonials.FindAsync(id);
                    if (item == null) return NotFound();

                    item.ClientName = model.ClientName;
                    item.Position = model.Position;
                    item.Feedback = model.Feedback;
                    item.YoutubeUrl = model.YoutubeUrl;
                    item.IsActive = model.IsActive;
                    item.ShowOnHome = model.ShowOnHome;
                    item.ProductNameAr = model.ProductNameAr;
                    item.ProductNameEn = model.ProductNameEn;
                    item.PortfolioId = model.PortfolioId;

                    // 1. معالجة حذف الصورة (Deferred)
                    if (deleteClientImage && !string.IsNullOrEmpty(item.ImagePath))
                    {
                         _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                         item.ImagePath = null;
                    }

                    // 2. معالجة حذف الفيديو (Deferred)
                    if (deleteClientVideo && !string.IsNullOrEmpty(item.VideoPath))
                    {
                        _fileService.DeleteFile(item.VideoPath, "testimonials/videos");
                        item.VideoPath = null;
                    }

                    // 2.be. معالجة حذف صورة الغلاف (Deferred)
                    if (deleteClientCoverImage && !string.IsNullOrEmpty(item.CoverImage))
                    {
                        _fileService.DeleteFile(item.CoverImage, "testimonials/covers");
                        item.CoverImage = null;
                    }

                    // 3. حفظ الصور الجديدة (مع حذف القديمة تلقائياً لو موجودة وما اتحذفتش لسه)
                    if (model.ImageFile != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.ImagePath)) _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                            item.ImagePath = await _fileService.SaveFileAsync(model.ImageFile, "testimonials/images");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading image in Edit Testimonial");
                            ModelState.AddModelError("ImageFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    // 4. حفظ الفيديو الجديد
                    if (model.VideoFile != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.VideoPath)) _fileService.DeleteFile(item.VideoPath, "testimonials/videos");
                            item.VideoPath = await _fileService.SaveFileAsync(model.VideoFile, "testimonials/videos");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading video in Edit Testimonial");
                            ModelState.AddModelError("VideoFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    // 5. حفظ صورة الغلاف الجديدة
                    if (model.CoverImageFile != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.CoverImage)) _fileService.DeleteFile(item.CoverImage, "testimonials/covers");
                            item.CoverImage = await _fileService.SaveFileAsync(model.CoverImageFile, "testimonials/covers");
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogError(ex, "Error uploading cover image in Edit Testimonial");
                            ModelState.AddModelError("CoverImageFile", ex.Message);
                            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                            return View(model);
                        }
                    }

                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    // Evict Home Cache
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);

                    TempData["SuccessMessage"] = "تم التحديث بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Edit));
                return Redirect("/Admin/Error/General");
            }
            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Testimonials.FindAsync(id);
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.ImagePath)) _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                    if (!string.IsNullOrEmpty(item.VideoPath)) _fileService.DeleteFile(item.VideoPath, "testimonials/videos");
                    if (!string.IsNullOrEmpty(item.CoverImage)) _fileService.DeleteFile(item.CoverImage, "testimonials/covers");

                    _context.Testimonials.Remove(item);
                    await _context.SaveChangesAsync();
                    
                    // Evict Home Cache
                    await _cacheStore.EvictByTagAsync("home_data", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Delete));
                return Redirect("/Admin/Error/General");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}