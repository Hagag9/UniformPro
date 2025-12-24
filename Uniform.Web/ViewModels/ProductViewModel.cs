using System.ComponentModel.DataAnnotations;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج بالعربية مطلوب")]
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون قيمة موجبة")]
        public decimal? StartPrice { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "اختيار القطاع مطلوب")]
        public int CategoryId { get; set; }

        public string MaterialDetailsAr { get; set; }
        public string MaterialDetailsEn { get; set; }
        public string AvailableSizes { get; set; }
        public int MinQuantity { get; set; } = 1;

        // --- حقول التعامل مع الصور ---

        // الصورة الأساسية (نستقبلها كملف)
        public IFormFile? MainImageFile { get; set; }

        // معرض الصور (نستقبلها كقائمة ملفات)
        public List<IFormFile>? GalleryFiles { get; set; }

        // --- حقول للعرض فقط في صفحة التعديل ---
        public string? CurrentMainImagePath { get; set; }
        public List<ProductImage>? CurrentGalleryImages { get; set; }

        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}