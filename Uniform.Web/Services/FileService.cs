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
            // مسار المجلد داخل wwwroot
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);

            // التأكد من وجود المجلد
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // إنشاء اسم فريد للملف لتجنب التكرار
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // الحفظ الفعلي
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        public void DeleteFile(string fileName, string folderName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", folderName, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}