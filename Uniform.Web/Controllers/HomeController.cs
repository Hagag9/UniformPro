using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;
using UniformPro.Core.Entities;
using Microsoft.AspNetCore.OutputCaching; // Added namespace

namespace UniformPro.Web.Controllers
{
    public class HomeController : FrontBaseController
    {
        public HomeController(ApplicationDbContext context) : base(context) { }

        [OutputCache(PolicyName = "HomePage")]
        public async Task<IActionResult> Index()
        {
            // جلب عناصر الـ Hero المفعلة مرتبة
            var heroItems = await _context.HeroItems
                .Where(h => h.IsActive)
                .OrderBy(h => h.DisplayOrder)
                .ToListAsync();

            // جلب المنتجات المميزة (التي حددها الأدمن)
            var featuredProducts = await _context.Products
                .Where(p => p.IsActive && p.ShowOnHome)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8) // Maximum 8 items if many are selected, or remove Take() if unlimited wanted
                .Include(p => p.Category)
                .ToListAsync();

            // Fallback: If no products selected for home, show latest 4 as before
            if (!featuredProducts.Any())
            {
                featuredProducts = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .Include(p => p.Category)
                    .ToListAsync();
            }


            // جلب أعمالنا السابقة المختارة للعرض في الرئيسية
            var homePortfolios = await _context.Portfolios
                .AsNoTracking()
                .Where(p => p.ShowOnHome)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new UniformPro.Web.ViewModels.PortfolioCardViewModel
                {
                    Id = p.Id,
                    ClientNameAr = p.ClientNameAr,
                    ClientNameEn = p.ClientNameEn,
                    CoverImagePath = p.CoverImagePath,
                    CategoryNameAr = p.Category.NameAr,
                    CategoryNameEn = p.Category.NameEn
                })
                .ToListAsync();

            // Fetch Happy Customers (Testimonials)
            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .Where(t => t.IsActive && t.ShowOnHome)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                SiteSettings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSettings(),
                HeroItems = heroItems,
                LatestProducts = featuredProducts,
                Portfolios = homePortfolios,
                Testimonials = testimonials
            };

            return View(viewModel);
        }

        public async Task<IActionResult> About()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (settings == null) return NotFound();
            return View(settings);
        }
    }
}
