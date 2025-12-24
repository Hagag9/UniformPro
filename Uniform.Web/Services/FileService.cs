using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using UniformPro.Web.Helpers;

namespace UniformPro.Web.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile imageFile, string folderName);
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
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Invalid file extension. Only .jpg, .jpeg, .png, .webp are allowed.");
            }

            // 2. Check MIME Type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(imageFile.ContentType.ToLowerInvariant()))
            {
                 throw new ArgumentException("Invalid MIME type.");
            }

            // 3. Check Magic Numbers (File Signature)
            using (var stream = imageFile.OpenReadStream())
            {
                if (!IsValidImageSignature(stream, fileExtension))
                {
                    throw new ArgumentException("Invalid file signature (Magic Number). This is not a valid image.");
                }
            }

            // مسار المجلد داخل wwwroot
            var uploadsFolder = Path.Combine(_environment.WebRootPath, Constants.Folders.Uploads, folderName);

            // التأكد من وجود المجلد
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Forced WebP extension
            var uniqueFileName = $"{Guid.NewGuid()}.webp";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // الحفظ باستخدام ImageSharp (Load -> Resize -> Save as WebP)
            using (var stream = imageFile.OpenReadStream())
            using (var image = await Image.LoadAsync(stream))
            {
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
    }
}