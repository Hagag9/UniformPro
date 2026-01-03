using System.ComponentModel.DataAnnotations;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class PortfolioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل بالعربية مطلوب")]
        [MaxLength(150, ErrorMessage = "الاسم يجب ألا يتجاوز 150 حرف")]
        public string ClientNameAr { get; set; }

        [Required(ErrorMessage = "Client Name in English is required")]
        [MaxLength(150, ErrorMessage = "Client Name must not exceed 150 characters")]
        public string ClientNameEn { get; set; }

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        [Required(ErrorMessage = "اختيار القطاع مطلوب")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "ترتيب العرض مطلوب")]
        public int DisplayOrder { get; set; }

        public bool ShowOnHome { get; set; }
        public bool IsActive { get; set; } = true;

        // --- الميديا ---
        public IFormFile? CoverImageFile { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
        public List<string>? YoutubeUrls { get; set; }

        // --- للعرض في التعديل ---
        public string? CurrentCoverImagePath { get; set; }
        public List<PortfolioMedia>? CurrentMedia { get; set; }

        [MaxLength(300, ErrorMessage = "الوصف المختصر يجب ألا يتجاوز 300 حرف")]
        public string? MetaDescription { get; set; }
        
        [MaxLength(200, ErrorMessage = "الكلمات المفتاحية يجب ألا تتجاوز 200 حرف")]
        public string? MetaKeywords { get; set; }
    }
}