
namespace UniformPro.Web.ViewModels
{
    public class PortfolioListViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? CoverImagePath { get; set; }
    }
}
