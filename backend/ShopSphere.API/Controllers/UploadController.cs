using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public UploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        // POST: api/upload/image
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var result = await _fileUploadService.UploadImageAsync(file);
            return Ok(result);
        }
    }
}