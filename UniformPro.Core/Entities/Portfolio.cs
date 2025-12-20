using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class Portfolio : BaseEntity
    {
       
        [Required, MaxLength(150)] public string ClientNameAr { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string ClientNameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

     
        public string? CoverImagePath { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // ✅  هل يظهر في سلايدر الصفحة الرئيسية؟
        public bool ShowOnHome { get; set; } = false;

        // ✅  ترتيب الظهور
        public int DisplayOrder { get; set; } = 0;
        public ICollection<PortfolioMedia> PortfolioMedias { get; set; } = new List<PortfolioMedia>();

        [MaxLength(300)]
        public string? MetaDescription { get; set; } // وصف يظهر تحت الرابط في جوجل

        [MaxLength(200)]
        public string? MetaKeywords { get; set; }    // كلمات مفتاحية (اختياري)
    }
}