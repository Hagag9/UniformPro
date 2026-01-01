using System.ComponentModel.DataAnnotations;

namespace UniformPro.Web.ViewModels
{
    public class TestimonialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        public string ClientName { get; set; }

        public string? Position { get; set; }
        
        [Display(Name = "اسم المنتج (عربي)")]
        public string? ProductNameAr { get; set; }

        [Display(Name = "اسم المنتج (إنجليزي)")]
        public string? ProductNameEn { get; set; }

        [Required(ErrorMessage = "نص التقييم مطلوب")]
        public string Feedback { get; set; }

        // ✅ ربط بمشروع (اختياري)
        [Display(Name = "المشروع المرتبط")]
        public int? PortfolioId { get; set; }

        public bool IsActive { get; set; } = true;
        public bool ShowOnHome { get; set; }

        // --- التعامل مع الملفات ---
        public IFormFile? ImageFile { get; set; } // صورة العميل
        public bool DeleteImage { get; set; }     // حذف الصورة الحالية

        public IFormFile? CoverImageFile { get; set; } // صورة الغلاف 
        public bool DeleteCoverImage { get; set; }     // حذف صورة الغلاف

        public IFormFile? VideoFile { get; set; } // ملف الفيديو
        public bool DeleteVideo { get; set; }     // حذف الفيديو الحالي
        
        public string? YoutubeUrl { get; set; }   // رابط يوتيوب

        // --- للعرض فقط (في التعديل) ---
        public string? CurrentImagePath { get; set; }
        public string? CurrentCoverImage { get; set; }    // ✅ اضافة
        public string? CurrentVideoPath { get; set; }
    }
}