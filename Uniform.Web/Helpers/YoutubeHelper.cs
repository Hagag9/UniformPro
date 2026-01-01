using System.Text.RegularExpressions;

namespace UniformPro.Core.Helpers
{
    public static class YoutubeHelper
    {
        public static string GetEmbedUrl(string originalUrl)
        {
            var videoId = GetVideoId(originalUrl);
            return !string.IsNullOrEmpty(videoId) ? $"https://www.youtube.com/embed/{videoId}" : originalUrl;
        }

        public static string GetThumbnailUrl(string originalUrl)
        {
            var videoId = GetVideoId(originalUrl);
            // mqdefault (320x180), hqdefault (480x360), maxresdefault (1280x720)
            return !string.IsNullOrEmpty(videoId) ? $"https://img.youtube.com/vi/{videoId}/mqdefault.jpg" : "";
        }

        public static string GetVideoId(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            
            var regex = new Regex(@"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?|shorts)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})");
            var match = regex.Match(url);
            
            return match.Success ? match.Groups[1].Value : "";
        }
    }
}