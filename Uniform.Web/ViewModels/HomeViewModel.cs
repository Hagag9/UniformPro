using System.Collections.Generic;
using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class HomeViewModel
    {
        public SiteSettings SiteSettings { get; set; } = new();
        public IEnumerable<HeroItem> HeroItems { get; set; } = new List<HeroItem>();
        public IEnumerable<Product> LatestProducts { get; set; } = new List<Product>();
        public IEnumerable<PortfolioCardViewModel> Portfolios { get; set; } = new List<PortfolioCardViewModel>();
        public IEnumerable<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
    }
}
