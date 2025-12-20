using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string ClientName { get; set; } // اسم العميل

        [MaxLength(100)]
        public string Position { get; set; } // الصفة: مدير مطعم X

        [Required]
        public string Feedback { get; set; } // نص التقييم

        // الميديا
        public string? ImagePath { get; set; } // صورة العميل
        public string? VideoPath { get; set; } // فيديو مرفوع
        public string? YoutubeUrl { get; set; } // رابط يوتيوب

        public bool IsActive { get; set; } = true; // تفعيل/إيقاف

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}