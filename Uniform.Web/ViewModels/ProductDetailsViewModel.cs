using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public decimal? StartPrice { get; set; }
        
        public string? MaterialDetailsAr { get; set; }
        public string? MaterialDetailsEn { get; set; }
        public int MinQuantity { get; set; }
        public string? AvailableSizes { get; set; }
        
        // Category
        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; }
        public string CategoryNameEn { get; set; }
        
        // Media
        public string? MainImagePath { get; set; }
        public List<ProductImage> ProductImages { get; set; } = new();

        // SEO
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        // Related Products
        public List<ProductListViewModel> RelatedProducts { get; set; } = new();
    }
}
