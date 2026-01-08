using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniformPro.Infrastructure.Data;
using UniformPro.Web.ViewModels;

using Microsoft.AspNetCore.OutputCaching;

namespace UniformPro.Web.Controllers
{
    public class DiscountPolicyController : FrontBaseController
    {
        public DiscountPolicyController(ApplicationDbContext context) : base(context)
        {
        }

        [OutputCache(PolicyName = "DiscountPage")]
        public async Task<IActionResult> Index()
        {
            // تحديد اللغة الحالية
            var isArabic = System.Globalization.CultureInfo.CurrentCulture.Name.StartsWith("ar");

            // جلب الشرائح المفعلة مرتبة
            var tiers = await _context.DiscountTiers
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            // تحويل للـ ViewModel
            var viewModel = new DiscountTiersPageViewModel
            {
                Tiers = tiers.Select(t => new DiscountTierViewModel
                {
                    Name = isArabic ? t.NameAr : t.NameEn,
                    DiscountPercentage = t.DiscountPercentage,
                    MinQuantity = t.MinQuantity,
                    MaxQuantity = t.MaxQuantity,
                    PromoText = isArabic ? t.PromoTextAr : t.PromoTextEn,
                    ColorCode = t.ColorCode,
                    Benefits = ParseBenefits(isArabic ? t.BenefitsAr : t.BenefitsEn)
                }).ToList()
            };

            return View(viewModel);
        }

        private List<string> ParseBenefits(string? benefits)
        {
            if (string.IsNullOrWhiteSpace(benefits))
                return new List<string>();

            return benefits
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();
        }
    }
}
