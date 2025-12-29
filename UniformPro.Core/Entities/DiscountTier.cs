using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class DiscountTier : BaseEntity
    {
        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        public decimal DiscountPercentage { get; set; }

        [Required]
        public int MinQuantity { get; set; }

        public int? MaxQuantity { get; set; }

        [MaxLength(200)]
        public string? PromoTextAr { get; set; }

        [MaxLength(200)]
        public string? PromoTextEn { get; set; }

        [Required, MaxLength(10)]
        public string ColorCode { get; set; } = "#008060";

        public int DisplayOrder { get; set; } = 0;

        [MaxLength(1000)]
        public string? BenefitsAr { get; set; }

        [MaxLength(1000)]
        public string? BenefitsEn { get; set; }
    }
}
