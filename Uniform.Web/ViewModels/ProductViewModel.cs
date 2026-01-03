using System.ComponentModel.DataAnnotations;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        [MaxLength(200, ErrorMessage = "الاسم يجب ألا يتجاوز 200 حرف")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "Name in English is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string NameEn { get; set; }

        public string? DescriptionAr { get; set; }

        public string? DescriptionEn { get; set; }

        public decimal? StartPrice { get; set; }

        [Required(ErrorMessage = "اختر التصنيف")]
        public int CategoryId { get; set; }

        public string? MaterialDetailsAr { get; set; }
        public string? MaterialDetailsEn { get; set; }

        [MaxLength(100, ErrorMessage = "المقاسات يجب ألا تتجاوز 100 حرف")]
        public string? AvailableSizes { get; set; }

        public int MinQuantity { get; set; } = 1;

        public bool ShowOnHome { get; set; } // عرض في الرئيسية

        [MaxLength(300, ErrorMessage = "الوصف المختصر يجب ألا يتجاوز 300 حرف")]
        public string? MetaDescription { get; set; }
        
        [MaxLength(200, ErrorMessage = "الكلمات المفتاحية يجب ألا تتجاوز 200 حرف")]
        public string? MetaKeywords { get; set; }

        // Images Input
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? GalleryFiles { get; set; }

        // Display properties
        public string? CurrentMainImagePath { get; set; }
        public List<ProductImage>? CurrentGalleryImages { get; set; }
    }
}