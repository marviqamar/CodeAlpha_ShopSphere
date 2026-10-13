using Microsoft.AspNetCore.Http;
using ShopSphere.Application.DTOs.Upload;

namespace ShopSphere.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<UploadResultDto> UploadImageAsync(IFormFile file);
    }
}