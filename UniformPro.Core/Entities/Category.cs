using System.ComponentModel.DataAnnotations;

namespace UniformPro.Core.Entities
{
    public class Category : BaseEntity
    {
        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
}