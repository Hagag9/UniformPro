using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    [AllowAnonymous]
    public class DiscountTiersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscountTiersController> _logger;

        public DiscountTiersController(ApplicationDbContext context, ILogger<DiscountTiersController> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(tier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم إضافة الشريحة بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Create));
                return Redirect("/Admin/Error/General");
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

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(tier);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "تم تعديل الشريحة بنجاح";
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!_context.DiscountTiers.Any(e => e.Id == tier.Id))
                            return NotFound();
                        else
                        {
                            _logger.LogError(ex, "Concurrency error in Edit DiscountTier");
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ActionName}", nameof(Edit));
                return Redirect("/Admin/Error/General");
            }
            return View(tier);
        }

        // الحذف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tier = await _context.DiscountTiers.FindAsync(id);
                if (tier != null)
                {
                    _context.DiscountTiers.Remove(tier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم حذف الشريحة بنجاح";
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
