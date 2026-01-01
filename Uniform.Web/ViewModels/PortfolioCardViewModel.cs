
namespace UniformPro.Web.ViewModels
{
    public class PortfolioCardViewModel
    {
        public int Id { get; set; }
        public string ClientNameAr { get; set; } = string.Empty;
        public string ClientNameEn { get; set; } = string.Empty;
        public string? CoverImagePath { get; set; }
        public string CategoryNameAr { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
    }
}
