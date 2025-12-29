using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;
using UniformPro.Web.ViewModels;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public TestimonialsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
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
            if (ModelState.IsValid)
            {
                // التحقق: يجب وجود فيديو مرفوع أو رابط يوتيوب
                if (model.VideoFile == null && string.IsNullOrEmpty(model.YoutubeUrl))
                {
                    ModelState.AddModelError("", "يجب إضافة فيديو (رفع ملف) أو رابط يوتيوب");
                    ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                    return View(model);
                }

                var testimonial = new Testimonial
                {
                    ClientName = model.ClientName,
                    Position = model.Position,
                    Feedback = model.Feedback,
                    YoutubeUrl = model.YoutubeUrl,
                    IsActive = model.IsActive,
                    ShowOnHome = model.ShowOnHome, // ✅ اضافة
                    CreatedAt = DateTime.Now,
                    PortfolioId = model.PortfolioId
                };

                // حفظ الصورة
                if (model.ImageFile != null)
                {
                    try
                    {
                        testimonial.ImagePath = await _fileService.SaveFileAsync(model.ImageFile, "testimonials/images");
                    }
                    catch (ArgumentException ex)
                    {
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
                        ModelState.AddModelError("VideoFile", ex.Message);
                        ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                        return View(model);
                    }
                }

                _context.Add(testimonial);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة التقييم بنجاح";
                return RedirectToAction(nameof(Index));
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
                CurrentImagePath = item.ImagePath,
                CurrentVideoPath = item.VideoPath,
                PortfolioId = item.PortfolioId
            };

            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", item.PortfolioId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestimonialViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var item = await _context.Testimonials.FindAsync(id);
                if (item == null) return NotFound();

                item.ClientName = model.ClientName;
                item.Position = model.Position;
                item.Feedback = model.Feedback;
                item.YoutubeUrl = model.YoutubeUrl;
                item.IsActive = model.IsActive;
                item.ShowOnHome = model.ShowOnHome; // ✅ اضافة
                item.PortfolioId = model.PortfolioId;

                // حفظ الصورة (مع حذف القديمة)
                if (model.ImageFile != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(item.ImagePath)) _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                        item.ImagePath = await _fileService.SaveFileAsync(model.ImageFile, "testimonials/images");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                        return View(model);
                    }
                }

                // حفظ الفيديو (مع حذف القديم)
                if (model.VideoFile != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(item.VideoPath)) _fileService.DeleteFile(item.VideoPath, "testimonials/videos");
                        item.VideoPath = await _fileService.SaveFileAsync(model.VideoFile, "testimonials/videos");
                    }
                    catch (ArgumentException ex)
                    {
                        ModelState.AddModelError("VideoFile", ex.Message);
                        ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
                        return View(model);
                    }
                }

                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم التحديث بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Portfolios = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Portfolios, "Id", "ClientNameAr", model.PortfolioId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMedia(int id, string type)
        {
            var item = await _context.Testimonials.FindAsync(id);
            if (item != null)
            {
                if (type == "image" && !string.IsNullOrEmpty(item.ImagePath))
                {
                    _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                    item.ImagePath = null;
                }
                else if (type == "video" && !string.IsNullOrEmpty(item.VideoPath))
                {
                    _fileService.DeleteFile(item.VideoPath, "testimonials/videos");
                    item.VideoPath = null;
                }
                
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Testimonials.FindAsync(id);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.ImagePath)) _fileService.DeleteFile(item.ImagePath, "testimonials/images");
                if (!string.IsNullOrEmpty(item.VideoPath)) _fileService.DeleteFile(item.VideoPath, "testimonials/videos");

                _context.Testimonials.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}