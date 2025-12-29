using System.ComponentModel.DataAnnotations;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "Name in English is required")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "وصف المنتج بالعربية مطلوب")]
        public string DescriptionAr { get; set; }

        [Required(ErrorMessage = "Product description in English is required")]
        public string DescriptionEn { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        public decimal? StartPrice { get; set; }

        [Required(ErrorMessage = "اختر التصنيف")]
        public int CategoryId { get; set; }

        public string? MaterialDetailsAr { get; set; }
        public string? MaterialDetailsEn { get; set; }

        public string? AvailableSizes { get; set; }

        public int MinQuantity { get; set; } = 1;

        public bool ShowOnHome { get; set; } // عرض في الرئيسية

        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Images Input
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? GalleryFiles { get; set; }

        // Display properties
        public string? CurrentMainImagePath { get; set; }
        public List<ProductImage>? CurrentGalleryImages { get; set; }
    }
}