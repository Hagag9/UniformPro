using System.Collections.Generic;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class ProductIndexViewModel
    {
        public IEnumerable<ProductListViewModel> Products { get; set; } = new List<ProductListViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        
        // Paginaton
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
        
        // Filters
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }
        
        // Settings
        public bool IsArabic { get; set; }
    }
}
