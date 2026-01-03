using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class HeroItem : BaseEntity
    {
        [Required]
        public string ImagePath { get; set; } = string.Empty; // صورة Desktop

        public string? MobileImagePath { get; set; } // صورة Mobile (للـ Responsive)

        [MaxLength(100)]
        public string? TitleAr { get; set; } // العنوان الرئيسي (اختياري)
        [MaxLength(100)]
        public string? TitleEn { get; set; }

        [MaxLength(200)]
        public string? SubtitleAr { get; set; } // الوصف الصغير
        [MaxLength(200)]
        public string? SubtitleEn { get; set; }

        public string? LinkUrl { get; set; } // زر "اطلب الآن" يودي فين؟
        public int DisplayOrder { get; set; } = 0; // للترتيب
        public bool HasOverlay { get; set; } = true; // طبقة شفافة
    }
}