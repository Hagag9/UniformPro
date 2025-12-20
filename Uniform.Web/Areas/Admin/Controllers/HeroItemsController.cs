using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.Services;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HeroItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public HeroItemsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
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
        public async Task<IActionResult> Create(HeroItem heroItem, IFormFile imageFile)
        {
            ModelState.Remove("ImagePath");
            if (imageFile == null)
            {
                ModelState.AddModelError("ImagePath", "يرجى رفع صورة للسلايدر");
            }

            if (ModelState.IsValid && imageFile != null)
            {
                heroItem.ImagePath = await _fileService.SaveFileAsync(imageFile, "hero");
                _context.Add(heroItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(heroItem);
        }

        // الحذف
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.HeroItems.FindAsync(id);
            if (item != null)
            {
                _fileService.DeleteFile(item.ImagePath, "hero");
                _context.HeroItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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
            catch
            {
                return BadRequest();
            }
        }
    }
}