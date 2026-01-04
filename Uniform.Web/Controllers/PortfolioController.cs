using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;
using Microsoft.AspNetCore.OutputCaching; // Added namespace
using UniformPro.Core.Entities;

namespace UniformPro.Web.Controllers
{
    public class PortfolioController : FrontBaseController
    {
        public PortfolioController(ApplicationDbContext context) : base(context) { }

        [OutputCache(PolicyName = "Portfolios")]
        public async Task<IActionResult> Index()
        {
             var isArabic = System.Globalization.CultureInfo.CurrentCulture.Name.StartsWith("ar");
             
             var items = await _context.Portfolios
                 .Include(p => p.Category)
                 .Where(p => p.IsActive)
                 .OrderBy(p => p.DisplayOrder)
                 .ThenByDescending(p => p.CreatedAt)
                 .Select(p => new PortfolioListViewModel {
                     Id = p.Id,
                     ClientName = isArabic ? p.ClientNameAr : p.ClientNameEn,
                     CategoryName = isArabic ? p.Category.NameAr : p.Category.NameEn,
                     CoverImagePath = p.CoverImagePath
                 })
                 .ToListAsync();

             return View(items);
        }

        [OutputCache(PolicyName = "PortfolioDetails")]
        public async Task<IActionResult> Details(int id)
        {
             var isArabic = System.Globalization.CultureInfo.CurrentCulture.Name.StartsWith("ar");
             
             var item = await _context.Portfolios
                 .Include(p => p.Category)
                 .Include(p => p.PortfolioMedias)
                 .Include(p => p.Testimonials) 
                 .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

             if (item == null) return NotFound();

             var model = new PortfolioDetailsViewModel
             {
                 Id = item.Id,
                 ClientName = isArabic ? item.ClientNameAr : item.ClientNameEn,
                 Description = isArabic ? item.DescriptionAr : item.DescriptionEn,
                 CoverImagePath = item.CoverImagePath,
                 CategoryName = isArabic ? item.Category.NameAr : item.Category.NameEn,
                 
                 GalleryImages = item.PortfolioMedias
                     .Where(m => m.Type == MediaType.Image)
                     .Select(m => m.MediaUrl).ToList(),

                 VideoUrls = item.PortfolioMedias
                     .Where(m => m.Type == MediaType.Video)
                     .Select(m => m.MediaUrl).ToList(),

                 ClientTestimonial = item.Testimonials.FirstOrDefault(t => t.IsActive),
                 
                 MetaDescription = item.MetaDescription,
                 MetaKeywords = item.MetaKeywords
             };

             return View(model);
        }
    }
}
