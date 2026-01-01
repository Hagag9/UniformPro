using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class DiscountTiersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountTiersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض كل الشرائح
        public async Task<IActionResult> Index()
        {
            var tiers = await _context.DiscountTiers
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
            return View(tiers);
        }

        // صفحة الإضافة (GET)
        public IActionResult Create()
        {
            return View();
        }

        // حفظ الإضافة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountTier tier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة الشريحة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(tier);
        }

        // صفحة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tier = await _context.DiscountTiers.FindAsync(id);
            if (tier == null) return NotFound();

            return View(tier);
        }

        // حفظ التعديل (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiscountTier tier)
        {
            if (id != tier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تعديل الشريحة بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DiscountTiers.Any(e => e.Id == tier.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tier);
        }

        // الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tier = await _context.DiscountTiers.FindAsync(id);
            if (tier != null)
            {
                _context.DiscountTiers.Remove(tier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الشريحة بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
