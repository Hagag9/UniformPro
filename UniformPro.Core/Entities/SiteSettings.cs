using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class SiteSettings
    {
        public int Id { get; set; } // سيكون صف واحد فقط (ID=1)

        [Display(Name = "اسم الموقع")]
        public string WebsiteNameAr { get; set; } = "يونيفورم برو";
        public string WebsiteNameEn { get; set; } = "Uniform Pro";

        // --- بيانات التواصل ---
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف الثاني")]
        public string? Phone2Number { get; set; }

        [Display(Name = "رقم الواتساب")]
        public string WhatsAppNumber { get; set; } = string.Empty; // بدون +

        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; } = string.Empty;

        // --- العناوين ---
        public string? AddressAr { get; set; } = string.Empty;
        public string? AddressEn { get; set; } = string.Empty;
        public string? MapLocationUrl { get; set; } = string.Empty; // رابط جوجل مابس

        // --- السوشيال ميديا ---
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YoutubeUrl { get; set; }  
        public string? TikTokUrl { get; set; }   

        // --- محتوى من نحن ---
        public string? AboutUsAr { get; set; }  // محتوى عربي (HTML من Summernote)
        public string? AboutUsEn { get; set; }  // محتوى إنجليزي (HTML)
        public string? OwnerImage { get; set; } // اسم ملف صورة المالك
        public string? LogoPath { get; set; }   // لوجو الموقع
    }
}