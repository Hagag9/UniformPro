namespace UniformPro.Core.Entities
{
    public enum MediaType
    {
        Image = 1,
        Video = 2 // (Youtube Link)
    }

    public class PortfolioMedia
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; } = string.Empty; // مسار الصورة أو لينك الفيديو
        public MediaType Type { get; set; }

        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; } = null!;
    }
}