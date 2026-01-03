using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniformPro.Infrastructure.Data;
using UniformPro.Core.Entities;
using UniformPro.Web.Helpers;

namespace UniformPro.Web.Services
{
    public class FileCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileCleanupBackgroundService> _logger;

        public FileCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment env,
            ILogger<FileCleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = _env.IsDevelopment() ? TimeSpan.FromSeconds(5) : TimeSpan.FromMinutes(3);
            _logger.LogInformation($"FileCleanupBackgroundService: Waiting {delay} before first run ({(_env.IsDevelopment() ? "Development" : "Production")} Mode)...");
            await Task.Delay(delay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("FileCleanupBackgroundService: Starting cleanup process...");
                    await Task.Run(() => CleanupOrphanFiles(), stoppingToken);
                    _logger.LogInformation("FileCleanupBackgroundService: Cleanup finished. Next run in 7 days.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FileCleanupBackgroundService: Error occurred during cleanup.");
                }

                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            }
        }

        private void CleanupOrphanFiles()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Products (Main + Gallery)
                var productMainFiles = context.Products
                    .Where(p => !string.IsNullOrEmpty(p.MainImagePath))
                    .Select(p => p.MainImagePath!)
                    .AsNoTracking().ToList();

                var productGalleryFiles = context.ProductImages
                    .Select(p => p.ImagePath)
                    .AsNoTracking().ToList();

                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "products"), new HashSet<string>(productMainFiles, StringComparer.OrdinalIgnoreCase), "Products Main");
                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "products", "gallery"), new HashSet<string>(productGalleryFiles, StringComparer.OrdinalIgnoreCase), "Products Gallery");

                // 2. Portfolios (Cover + Media)
                var portfolioCovers = context.Portfolios
                    .Where(p => !string.IsNullOrEmpty(p.CoverImagePath))
                    .Select(p => p.CoverImagePath!)
                    .AsNoTracking().ToList();

                var portfolioMedia = context.PortfolioMedias
                    .Select(p => p.MediaUrl)
                    .AsNoTracking().ToList();

                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "portfolios", "covers"), new HashSet<string>(portfolioCovers, StringComparer.OrdinalIgnoreCase), "Portfolio Covers");
                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "portfolios", "media"), new HashSet<string>(portfolioMedia, StringComparer.OrdinalIgnoreCase), "Portfolio Media");

                // 3. Testimonials (Image, Cover, Video)
                var testImages = context.Testimonials.Where(t => !string.IsNullOrEmpty(t.ImagePath)).Select(t => t.ImagePath!).AsNoTracking().ToList();
                var testCovers = context.Testimonials.Where(t => !string.IsNullOrEmpty(t.CoverImage)).Select(t => t.CoverImage!).AsNoTracking().ToList();
                var testVideos = context.Testimonials.Where(t => !string.IsNullOrEmpty(t.VideoPath)).Select(t => t.VideoPath!).AsNoTracking().ToList();

                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "testimonials", "images"), new HashSet<string>(testImages, StringComparer.OrdinalIgnoreCase), "Testimonial Images");
                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "testimonials", "covers"), new HashSet<string>(testCovers, StringComparer.OrdinalIgnoreCase), "Testimonial Covers");
                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "testimonials", "videos"), new HashSet<string>(testVideos, StringComparer.OrdinalIgnoreCase), "Testimonial Videos");

                // 4. Hero Items
                var heroFiles = context.HeroItems.Where(h => !string.IsNullOrEmpty(h.ImagePath)).Select(h => h.ImagePath).AsNoTracking().ToList();
                var heroMobileFiles = context.HeroItems.Where(h => !string.IsNullOrEmpty(h.MobileImagePath)).Select(h => h.MobileImagePath!).AsNoTracking().ToList();
                var allHeroFiles = heroFiles.Concat(heroMobileFiles).ToList();

                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "hero"), new HashSet<string>(allHeroFiles, StringComparer.OrdinalIgnoreCase), "Hero");

                // 5. Site Settings (Logo, OwnerImage)
                var settings = context.SiteSettings.AsNoTracking().FirstOrDefault();
                var settingsFiles = new List<string>();
                if (settings != null)
                {
                    if (!string.IsNullOrEmpty(settings.LogoPath)) settingsFiles.Add(settings.LogoPath);
                    if (!string.IsNullOrEmpty(settings.OwnerImage)) settingsFiles.Add(settings.OwnerImage);
                }
                CleanupFolder(Path.Combine(_env.WebRootPath, "uploads", "sitesettings"), new HashSet<string>(settingsFiles, StringComparer.OrdinalIgnoreCase), "SiteSettings");
            }
        }

        private void CleanupFolder(string folderPath, HashSet<string> validFiles, string label)
        {
            if (!Directory.Exists(folderPath)) return;

            try
            {
                var files = Directory.EnumerateFiles(folderPath); // Lazy enumeration
                int deleted = 0;

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);
                   
                    if (!validFiles.Contains(fileName))
                    {
                        try
                        {
                            File.Delete(filePath);
                            deleted++;
                            _logger.LogInformation($"[Cleanup] Deleted orphan file: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"[Cleanup] Failed to delete file {fileName}: {ex.Message}");
                        }
                    }
                }

                if (deleted > 0)
                {
                    _logger.LogInformation($"[{label}] Cleanup Complete. Removed {deleted} orphan files.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accessing folder: {folderPath}");
            }
        }
    }
}
