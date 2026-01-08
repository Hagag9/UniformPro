using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;
using UniformPro.Core.Entities;
using Microsoft.AspNetCore.OutputCaching;

namespace UniformPro.Web.Controllers
{
    public class HomeController : FrontBaseController
    {
        public HomeController(ApplicationDbContext context) : base(context) { }

        [OutputCache(PolicyName = "HomePage")]
        public async Task<IActionResult> Index()
        {
            // 1. Hero Items: Added AsNoTracking
            var heroItems = await _context.HeroItems
                .AsNoTracking()
                .Where(h => h.IsActive)
                .OrderBy(h => h.DisplayOrder)
                .ToListAsync();

            // 2. Featured Products: Added AsNoTracking
            var featuredProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.ShowOnHome)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Include(p => p.Category)
                .ToListAsync();

            // Fallback Logic
            if (!featuredProducts.Any())
            {
                featuredProducts = await _context.Products
                    .AsNoTracking() // Don't forget it here too
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .Include(p => p.Category)
                    .ToListAsync();
            }

            // 3. Portfolios: This was already perfect (Good job!)
            var homePortfolios = await _context.Portfolios
                .AsNoTracking()
                .Where(p => p.ShowOnHome && p.IsActive)
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

            // 4. Testimonials: Added AsNoTracking (just to be safe, though you had logic right)
            var testimonials = await _context.Testimonials
                .AsNoTracking()
                .Where(t => t.IsActive && t.ShowOnHome)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // 5. SiteSettings: Added AsNoTracking
            var settings = await _context.SiteSettings
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? new SiteSettings();

            var viewModel = new HomeViewModel
            {
                SiteSettings = settings,
                HeroItems = heroItems,
                LatestProducts = featuredProducts,
                Portfolios = homePortfolios,
                Testimonials = testimonials,
                ClientLogos = await _context.Testimonials
                    .AsNoTracking()
                    .Where(t => t.IsActive && !string.IsNullOrEmpty(t.ImagePath))
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.ImagePath)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [OutputCache(PolicyName = "AboutPage")]
        public async Task<IActionResult> About()
        {
            var settings = await _context.SiteSettings
                .AsNoTracking() // Important
                .FirstOrDefaultAsync();

            if (settings == null) return NotFound();
            return View(settings);
        }
    }
}