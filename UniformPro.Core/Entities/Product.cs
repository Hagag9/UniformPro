using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniformPro.Core.Entities
{
    public class Product : BaseEntity
    {
        // ... (الاسم عربي وانجليزي كما هم) ...
        [Required, MaxLength(200)] public string NameAr { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string NameEn { get; set; } = string.Empty;

        // ✅ 1. إضافة الوصف الإنجليزي
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? StartPrice { get; set; }

        // ✅ 2. إضافة أقل كمية للطلب (MOQ)
        public int MinQuantity { get; set; } = 1; // القيمة الافتراضية 1

        // ✅ 3. المقاسات المتاحة (نص حر يكتبه الأدمن: S, M, L, XL)
        [MaxLength(100)]
        public string? AvailableSizes { get; set; }

        // ✅ 4. تفاصيل الخامة (تم تعديلها لتكون أوضح)
        public string? MaterialDetailsAr { get; set; }
        public string? MaterialDetailsEn { get; set; }

        public string? MainImagePath { get; set; } // الصورة الأساسية فقط

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // علاقة صور المعرض
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        [MaxLength(300)]
        public string? MetaDescription { get; set; } // وصف يظهر تحت الرابط في جوجل

        [MaxLength(200)]
        public string? MetaKeywords { get; set; }    // كلمات مفتاحية (اختياري)
    }
}