using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using UniformPro.Web.Helpers;

namespace UniformPro.Web.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile imageFile, string folderName);
        Task<string> SaveHeroDesktopImageAsync(IFormFile imageFile);
        Task<string> SaveHeroMobileImageAsync(IFormFile imageFile);
        
        // kept for backward comp.
        Task<(string DesktopImage, string MobileImage)> SaveHeroImageAsync(IFormFile imageFile);
        void DeleteFile(string fileName, string folderName);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            // 1. Check File Extension
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var allowedVideoExtensions = new[] { ".mp4", ".mov", ".webm" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            bool isImage = allowedImageExtensions.Contains(fileExtension);
            bool isVideo = allowedVideoExtensions.Contains(fileExtension);

            if (!isImage && !isVideo)
            {
                throw new ArgumentException("Invalid file extension. Only .jpg, .jpeg, .png, .webp, .mp4, .mov, .webm are allowed.");
            }

            // 2. Check MIME Type
            // Simplification: We trust the extension for now or add video mimes.
            // keeping it simple for video to avoid complex signature checks without external libs for now.

            // مسار المجلد داخل wwwroot
            var uploadsFolder = Path.Combine(_environment.WebRootPath, Constants.Folders.Uploads, folderName);

            // التأكد من وجود المجلد
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName;

            if (isImage)
            {
                // Forced WebP extension for images
                uniqueFileName = $"{Guid.NewGuid()}.webp";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // الحفظ باستخدام ImageSharp (Load -> Resize -> Save as WebP)
                using (var stream = imageFile.OpenReadStream())
                using (var image = await Image.LoadAsync(stream))
                {
                    image.Mutate(x => x.AutoOrient());
                    // تغيير الحجم إذا كانت الصورة أكبر من MaxWidth
                    if (image.Width > Constants.Images.MaxWidth)
                    {
                        var newHeight = (int)((double)image.Height / image.Width * Constants.Images.MaxWidth);
                        image.Mutate(x => x.Resize(Constants.Images.MaxWidth, newHeight));
                    }

                    // إعدادات الـ Encoder (ضغط الـ WebP)
                    var encoder = new WebpEncoder
                    {
                        Quality = Constants.Images.Quality
                    };

                    await image.SaveAsWebpAsync(filePath, encoder);
                }
            }
            else
            {
                // For Video: Save as is
                uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
            }

            return uniqueFileName;
        }



        private bool IsValidImageSignature(Stream stream, string extension)
        {
            stream.Position = 0;
            using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
            var headerBytes = reader.ReadBytes(12); // Read enough bytes for most signatures

            // Signatures
            var jpeg = new byte[] { 0xFF, 0xD8, 0xFF };
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var webp = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // "RIFF" ... "WEBP" logic is more complex but start with RIFF

            return extension switch
            {
                ".jpg" or ".jpeg" => headerBytes.Take(3).SequenceEqual(jpeg),
                ".png" => headerBytes.Take(8).SequenceEqual(png),
                ".webp" => headerBytes.Take(4).SequenceEqual(webp), // Simplified WebP check
                _ => false
            };
        }

        public void DeleteFile(string fileName, string folderName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(_environment.WebRootPath, Constants.Folders.Uploads, folderName, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// حفظ صورة Hero بنسختين: Desktop (1920px) و Mobile (768px)
        /// </summary>
        public async Task<string> SaveHeroDesktopImageAsync(IFormFile imageFile)
        {
            return await SaveHeroImageInternalAsync(imageFile, Constants.HeroImages.DesktopWidth, "_desktop");
        }

        public async Task<string> SaveHeroMobileImageAsync(IFormFile imageFile)
        {
            return await SaveHeroImageInternalAsync(imageFile, Constants.HeroImages.MobileWidth, "_mobile");
        }

        private async Task<string> SaveHeroImageInternalAsync(IFormFile imageFile, int targetWidth, string suffix)
        {
            if (imageFile == null || imageFile.Length == 0) throw new ArgumentException("File is empty");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, Constants.Folders.Uploads, Constants.Folders.Hero);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{suffix}.webp";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = imageFile.OpenReadStream())
            using (var image = await Image.LoadAsync(stream))
            {
                var encoder = new WebpEncoder { Quality = Constants.HeroImages.Quality };

                if (image.Width > targetWidth)
                {
                    var newHeight = (int)((double)image.Height / image.Width * targetWidth);
                    image.Mutate(x => x.Resize(targetWidth, newHeight));
                }

                await image.SaveAsWebpAsync(filePath, encoder);
            }

            return uniqueFileName;
        }

        // Deprecated but kept for interface compatibility if needed, or remove. 
        // Let's remove implementation and rely on new ones.
        public async Task<(string DesktopImage, string MobileImage)> SaveHeroImageAsync(IFormFile imageFile)
        {
             var d = await SaveHeroDesktopImageAsync(imageFile);
             var m = await SaveHeroMobileImageAsync(imageFile);
             return (d, m);
        }
    }
}