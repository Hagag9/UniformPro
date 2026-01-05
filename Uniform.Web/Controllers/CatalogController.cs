using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UniformPro.Infrastructure.Data;

namespace UniformPro.Web.Controllers
{
    [ApiController]
    [Route("catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("products")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache for 1 hour
        public async Task<IActionResult> GetProductsFeed()
        {
            var baseUrl = GetBaseUrl();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive) // Use IsActive instead of IsDeleted
                .ToListAsync();

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var siteName = settings?.WebsiteNameAr ?? "UniformPro";

            // XML Structure
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\"?>");
            xml.AppendLine("<rss xmlns:g=\"http://base.google.com/ns/1.0\" version=\"2.0\">");
            xml.AppendLine("<channel>");
            xml.AppendLine($"<title>{siteName} Products</title>");
            xml.AppendLine($"<link>{baseUrl}</link>");
            xml.AppendLine($"<description>Product feed for {siteName}</description>");

            foreach (var product in products)
            {
                var price = product.StartPrice.HasValue ? product.StartPrice.Value : 0;
                var desc = StripHtml(product.DescriptionAr ?? product.NameAr);
                var imageUrl = !string.IsNullOrEmpty(product.MainImagePath)
                    ? $"{baseUrl}/uploads/products/{product.MainImagePath}"
                    : $"{baseUrl}/images/no-image.png";

                xml.AppendLine("<item>");
                xml.AppendLine($"<g:id>{product.Id}</g:id>");
                xml.AppendLine($"<g:title><![CDATA[{product.NameAr}]]></g:title>");
                xml.AppendLine($"<g:description><![CDATA[{desc}]]></g:description>");
                xml.AppendLine($"<g:link>{baseUrl}/Products/Details/{product.Id}</g:link>");
                xml.AppendLine($"<g:image_link>{imageUrl}</g:image_link>");
                xml.AppendLine($"<g:brand>{siteName}</g:brand>");
                xml.AppendLine($"<g:condition>new</g:condition>");
                xml.AppendLine($"<g:availability>{(price > 0 ? "in stock" : "in stock")}</g:availability>"); // Always in stock usually
                xml.AppendLine($"<g:price>{price} EGP</g:price>");
                // Optional: Category, Google Product Category, etc.
                xml.AppendLine("</item>");
            }

            xml.AppendLine("</channel>");
            xml.AppendLine("</rss>");

            return Content(xml.ToString(), "application/xml", Encoding.UTF8);
        }

        [HttpGet("portfolio")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetPortfolioFeed()
        {
            var baseUrl = GetBaseUrl();
            var items = await _context.Portfolios
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

             var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var siteName = settings?.WebsiteNameAr ?? "UniformPro";

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\"?>");
            xml.AppendLine("<rss xmlns:g=\"http://base.google.com/ns/1.0\" version=\"2.0\">");
            xml.AppendLine("<channel>");
            xml.AppendLine($"<title>{siteName} Portfolio</title>");
            xml.AppendLine($"<link>{baseUrl}</link>");
            xml.AppendLine($"<description>Portfolio feed for {siteName}</description>");

            foreach (var item in items)
            {
                // Requirements: Price 0, "in stock"
                var desc = StripHtml(item.DescriptionAr ?? item.ClientNameAr);
                var imageUrl = !string.IsNullOrEmpty(item.CoverImagePath)
                    ? $"{baseUrl}/uploads/portfolios/{item.CoverImagePath}"
                    : $"{baseUrl}/images/no-image.png";

                xml.AppendLine("<item>");
                xml.AppendLine($"<g:id>{item.Id}</g:id>");
                xml.AppendLine($"<g:title><![CDATA[{item.ClientNameAr}]]></g:title>");
                xml.AppendLine($"<g:description><![CDATA[{desc}]]></g:description>");
                xml.AppendLine($"<g:link>{baseUrl}/Portfolio/Details/{item.Id}</g:link>");
                xml.AppendLine($"<g:image_link>{imageUrl}</g:image_link>");
                xml.AppendLine($"<g:brand>{siteName}</g:brand>");
                xml.AppendLine($"<g:condition>new</g:condition>");
                xml.AppendLine($"<g:availability>in stock</g:availability>");
                xml.AppendLine($"<g:price>0 EGP</g:price>");
                xml.AppendLine("</item>");
            }

            xml.AppendLine("</channel>");
            xml.AppendLine("</rss>");

            return Content(xml.ToString(), "application/xml", Encoding.UTF8);
        }

        // Helpers
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return "https://uniformpro.com"; // Fallback
            return $"{request.Scheme}://{request.Host}";
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Remove scripts and styles
            input = Regex.Replace(input, @"<script.*?>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<style.*?>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            // Remove tags
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}
