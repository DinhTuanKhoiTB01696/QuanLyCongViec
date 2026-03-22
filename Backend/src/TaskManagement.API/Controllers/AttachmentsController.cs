using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using TaskManagement.API.Filters;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;

        public AttachmentsController(IFileService fileService, ApplicationDbContext context)
        {
            _fileService = fileService;
            _context = context;
        }

        [HttpPost("upload/{taskId}")]
        [ProjectAuthorize("Admin,PM,DEV,Developer,PROJECT_MANAGER")]
        public async Task<IActionResult> Upload(Guid projectId, Guid taskId, IFormFile file)
        {
            var task = await _context.WorkTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Security: In a real app, check if user has permission for this task's project
            
            using (var stream = file.OpenReadStream())
            {
                var savedFileName = await _fileService.UploadFileAsync(stream, file.FileName);
                if (string.IsNullOrEmpty(savedFileName)) return BadRequest("File upload failed.");

                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    WorkTaskId = taskId,
                    UserId = userId,
                    FileName = file.FileName,
                    FileUrl = savedFileName, // This is the Guid-based name
                    FileSize = file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                return Ok(attachment);
            }
        }

        [HttpGet("{attachmentId}")]
        public async Task<IActionResult> Download(Guid taskId, Guid attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null || attachment.WorkTaskId != taskId) return NotFound();

            var fileData = await _fileService.DownloadFileAsync(attachment.FileUrl);
            if (fileData == null) return NotFound("Physical file not found.");

            return File(fileData.Value.Bytes, fileData.Value.ContentType, attachment.FileName);
        }
    }
}
