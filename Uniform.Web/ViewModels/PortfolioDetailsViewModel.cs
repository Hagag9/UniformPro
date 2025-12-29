using UniformPro.Core.Entities;

namespace UniformPro.Web.ViewModels
{
    public class PortfolioDetailsViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; } // The Master Image (Cover)
        public string CategoryName { get; set; } = string.Empty;

        // Gallery Images (Master Image excluded if handled separately, or included if part of gallery)
        // User said: "Show the 5 or 6 images of the session" in gallery.
        public List<string> GalleryImages { get; set; } = new List<string>();

        // Videos Section
        public List<string> VideoUrls { get; set; } = new List<string>();

        // Testimonial attached to this project
        public Testimonial? ClientTestimonial { get; set; } 

        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}
