using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;


namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly string _uploadFolder;

        public UploadsController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _uploadFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (Array.IndexOf(allowedExtensions, extension) < 0)
            {
                return BadRequest(new { message = "Invalid file type. Only images are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { message = "File size exceeds 5MB limit." });
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";

            return Ok(new { url });
        }

        [HttpPost("file")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            if (file.Length > 20 * 1024 * 1024) // 20MB limit
            {
                return BadRequest(new { message = "File size exceeds 20MB limit." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";

            return Ok(new { url, name = file.FileName, size = file.Length });
        }
    }
}
