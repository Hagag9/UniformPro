using Ganss.Xss;

namespace UniformPro.Web.Services
{
    public interface IHtmlSanitizerService
    {
        string Sanitize(string html);
    }

    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer;
        
        public HtmlSanitizerService()
        {
            _sanitizer = new HtmlSanitizer();
            // Allow safe tags only - extending default list if needed, or restricting it
            // Default allowed tags are safe enough, but we can be explicit
            
            // We'll trust the default configuration for now but ensure we can customize if needed
            // Default allows simple formatting, links, tables, etc.
            // It strips scripts, iframes, objects, onclick handlers, etc.
        }
        
        public string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            return _sanitizer.Sanitize(html);
        }
    }
}
