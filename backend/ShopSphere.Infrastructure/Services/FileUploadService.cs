using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ShopSphere.Application.DTOs.Upload;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService

    {
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<UploadResultDto> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException("No file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                throw new ValidationException("File size must not exceed 5 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ValidationException("Only JPG, PNG, WEBP, and GIF images are allowed.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new UploadResultDto
            {
                Url = $"/uploads/{uniqueFileName}"
            };
        }
    }
}