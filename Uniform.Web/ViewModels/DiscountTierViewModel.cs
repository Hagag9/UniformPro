namespace UniformPro.Web.ViewModels
{
    public class DiscountTierViewModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public int MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public string? PromoText { get; set; }
        public string ColorCode { get; set; } = "#008060";
        public List<string> Benefits { get; set; } = new();
    }

    public class DiscountTiersPageViewModel
    {
        public List<DiscountTierViewModel> Tiers { get; set; } = new();
    }
}
