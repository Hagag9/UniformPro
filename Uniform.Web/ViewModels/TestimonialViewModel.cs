using System.ComponentModel.DataAnnotations;

namespace UniformPro.Web.ViewModels
{
    public class TestimonialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
        public string Position { get; set; }

        [Required(ErrorMessage = "نص التقييم مطلوب")]
        public string Feedback { get; set; }

        public bool IsActive { get; set; } = true;

        // --- التعامل مع الملفات ---
        public IFormFile? ImageFile { get; set; } // صورة العميل
        public IFormFile? VideoFile { get; set; } // ملف الفيديو
        public string? YoutubeUrl { get; set; }   // رابط يوتيوب

        // --- للعرض فقط (في التعديل) ---
        public string? CurrentImagePath { get; set; }
        public string? CurrentVideoPath { get; set; }
    }
}