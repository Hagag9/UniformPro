using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Core.Entities;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Areas.Admin.Controllers
{
    [Area("Admin")] // ضروري جداً لتحديد المنطقة
    //[Authorize]     // يمنع الدخول إلا للمسجلين
    [AllowAnonymous]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض كل الأقسام
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // صفحة الإضافة (GET)
        public IActionResult Create()
        {
            return View();
        }

        // حفظ الإضافة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // صفحة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // حفظ التعديل (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // الحذف
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // 1. التحقق من وجود منتجات مرتبطة
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "عفواً، لا يمكن حذف هذا القسم لأنه يحتوي على منتجات. يرجى نقل المنتجات أو حذفها أولاً.";
                return RedirectToAction(nameof(Index));
            }

            // 2. التحقق من وجود مشاريع مرتبطة
            var hasPortfolios = await _context.Portfolios.AnyAsync(p => p.CategoryId == id);
            if (hasPortfolios)
            {
                TempData["ErrorMessage"] = "عفواً، لا يمكن حذف هذا القسم لأنه مستخدم في معرض الأعمال. يرجى تعديل المشاريع المرتبطة أولاً.";
                return RedirectToAction(nameof(Index));
            }

            // 3. الحذف الآمن
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حذف القسم بنجاح";
            
            return RedirectToAction(nameof(Index));
        }
    }
}