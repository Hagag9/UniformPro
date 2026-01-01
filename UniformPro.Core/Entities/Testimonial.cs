using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string ClientName { get; set; } // اسم العميل

        [MaxLength(100)]
        public string? Position { get; set; } // الصفة: مدير مطعم X

        [MaxLength(100)]
        public string? ProductNameAr { get; set; } // اسم المنتج (عربي)

        [MaxLength(100)]
        public string? ProductNameEn { get; set; } // اسم المنتج (نجليزي)

        // ✅ ربط مع المشاريع (اختياري)
        public int? PortfolioId { get; set; }
        public Portfolio? Portfolio { get; set; }

        [Required]
        public string Feedback { get; set; } // نص التقييم

        // الميديا
        public string? ImagePath { get; set; } // صورة العميل
        public string? CoverImage { get; set; } // صورة الغلاف (Video Cover)
        public string? VideoPath { get; set; } // فيديو مرفوع
        public string? YoutubeUrl { get; set; } // رابط يوتيوب

        public bool IsActive { get; set; } = true; // تفعيل/إيقاف
        
        // هل يظهر في الرئيسية؟
        public bool ShowOnHome { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}