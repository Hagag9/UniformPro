using System.ComponentModel.DataAnnotations;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class PortfolioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل بالعربية مطلوب")]
        public string ClientNameAr { get; set; }
        public string ClientNameEn { get; set; }

        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "اختيار القطاع مطلوب")]
        public int CategoryId { get; set; }

        public bool ShowOnHome { get; set; }

        // --- الميديا ---
        public IFormFile? CoverImageFile { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
        public List<string>? YoutubeUrls { get; set; }

        // --- للعرض في التعديل ---
        public string? CurrentCoverImagePath { get; set; }
        public List<PortfolioMedia>? CurrentMedia { get; set; }

        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}