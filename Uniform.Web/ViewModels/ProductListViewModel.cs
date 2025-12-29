namespace UniformPro.Web.ViewModels
{
    public class ProductListViewModel
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string? MainImagePath { get; set; }
        public string? MaterialDetailsAr { get; set; }
        public string? MaterialDetailsEn { get; set; }
        public decimal? StartPrice { get; set; }
    }
}
