using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/workspaces/{workspaceId}/[controller]")]
    [Authorize]
    public class GoalsController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalsController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid workspaceId)
        {
            var result = await _goalService.GetAllAsync(workspaceId);
            return Ok(result);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses(Guid workspaceId)
        {
            var result = await _goalService.GetStatusesAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid workspaceId, Guid id)
        {
            var result = await _goalService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid workspaceId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.CreateAsync(userId, workspaceId, dto);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var result = await _goalService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid workspaceId, Guid id)
        {
            await _goalService.ArchiveAsync(id);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid workspaceId, Guid id)
        {
            await _goalService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/updates")]
        public async Task<IActionResult> AddUpdate(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddUpdateAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/lessons")]
        public async Task<IActionResult> AddLesson(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddLessonAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/risks")]
        public async Task<IActionResult> AddRisk(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddRiskAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPost("{id:guid}/decisions")]
        public async Task<IActionResult> AddDecision(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddDecisionAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpPut("{id:guid}/updates/{updateId:guid}")]
        public async Task<IActionResult> UpdateUpdate(Guid workspaceId, Guid id, Guid updateId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateUpdateAsync(id, updateId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/updates/{updateId:guid}")]
        public async Task<IActionResult> DeleteUpdate(Guid workspaceId, Guid id, Guid updateId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteUpdateAsync(id, updateId, userId);
            return NoContent();
        }

        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid workspaceId, Guid id)
        {
            var result = await _goalService.GetCommentsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddCommentAsync(id, userId, dto);
            return Ok(result);
        }

        [HttpGet("{id:guid}/updates/{updateId:guid}/comments")]
        public async Task<IActionResult> GetUpdateComments(Guid workspaceId, Guid id, Guid updateId)
        {
            var result = await _goalService.GetUpdateCommentsAsync(updateId);
            return Ok(result);
        }

        [HttpPost("{id:guid}/updates/{updateId:guid}/comments")]
        public async Task<IActionResult> AddUpdateComment(Guid workspaceId, Guid id, Guid updateId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.AddUpdateCommentAsync(id, updateId, userId, dto);
            return Ok(result);
        }

        [HttpPut("{id:guid}/updates/{updateId:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> UpdateUpdateComment(Guid workspaceId, Guid id, Guid updateId, Guid commentId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateCommentAsync(commentId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/updates/{updateId:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> DeleteUpdateComment(Guid workspaceId, Guid id, Guid updateId, Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteCommentAsync(commentId, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> UpdateComment(Guid workspaceId, Guid id, Guid commentId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateCommentAsync(commentId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/comments/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid workspaceId, Guid id, Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteCommentAsync(commentId, userId);
            return NoContent();
        }

        [HttpGet("{id:guid}/updates/{updateId:guid}/reactions")]
        public async Task<IActionResult> GetReactions(Guid workspaceId, Guid id, Guid updateId)
        {
            var result = await _goalService.GetReactionsAsync(updateId);
            return Ok(result);
        }

        [HttpPost("{id:guid}/updates/{updateId:guid}/reactions")]
        public async Task<IActionResult> ToggleReaction(Guid workspaceId, Guid id, Guid updateId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(System.Text.Json.JsonSerializer.Serialize(dto));
            var type = data.TryGetProperty("reactionType", out var rt) ? rt.GetString() : null;

            var validReactions = new[] { "like", "heart", "smile", "tada", "eyes" };
            if (string.IsNullOrEmpty(type) || !validReactions.Contains(type))
            {
                return BadRequest(new { statusCode = 400, message = "Invalid reactionType." });
            }

            var result = await _goalService.ToggleReactionAsync(updateId, userId, dto);
            return Ok(result);
        }

        [HttpGet("{id:guid}/updates/{updateId:guid}/attachments")]
        public async Task<IActionResult> GetUpdateAttachments(Guid workspaceId, Guid id, Guid updateId)
        {
            var result = await _goalService.GetUpdateAttachmentsAsync(updateId);
            return Ok(result);
        }

        [HttpPost("{id:guid}/updates/{updateId:guid}/attachments")]
        public async Task<IActionResult> UploadUpdateAttachment(Guid workspaceId, Guid id, Guid updateId, IFormFile file)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            if (file == null || file.Length == 0) return BadRequest(new { statusCode = 400, message = "No file uploaded" });

            // File Validation
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { statusCode = 400, message = "Invalid file type. Only JPG, PNG, WEBP, and GIF are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { statusCode = 400, message = "File size exceeds the 5MB limit." });
            }

            var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "goals");
            System.IO.Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/goals/{uniqueFileName}";
            var result = await _goalService.AddUpdateAttachmentAsync(updateId, userId, file.FileName, fileUrl, file.Length);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/updates/{updateId:guid}/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DeleteUpdateAttachment(Guid workspaceId, Guid id, Guid updateId, Guid attachmentId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteUpdateAttachmentAsync(attachmentId, userId);
            return NoContent();
        }

        [HttpGet("{id:guid}/projects")]
        public async Task<IActionResult> GetProjectLinks(Guid workspaceId, Guid id, [FromQuery] string linkType = null)
        {
            var result = await _goalService.GetProjectLinksAsync(id, linkType);
            return Ok(result);
        }

        [HttpPost("{id:guid}/projects")]
        public async Task<IActionResult> AddProjectLink(Guid workspaceId, Guid id, [FromBody] object dto)
        {
            try 
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var result = await _goalService.AddProjectLinkAsync(id, userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/projects/{linkId:guid}")]
        public async Task<IActionResult> DeleteProjectLink(Guid workspaceId, Guid id, Guid linkId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteProjectLinkAsync(id, linkId, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}/lessons/{lessonId:guid}")]
        public async Task<IActionResult> UpdateLesson(Guid workspaceId, Guid id, Guid lessonId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateLessonAsync(id, lessonId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/lessons/{lessonId:guid}")]
        public async Task<IActionResult> DeleteLesson(Guid workspaceId, Guid id, Guid lessonId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteLessonAsync(id, lessonId, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}/risks/{riskId:guid}")]
        public async Task<IActionResult> UpdateRisk(Guid workspaceId, Guid id, Guid riskId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateRiskAsync(id, riskId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/risks/{riskId:guid}")]
        public async Task<IActionResult> DeleteRisk(Guid workspaceId, Guid id, Guid riskId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteRiskAsync(id, riskId, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}/decisions/{decisionId:guid}")]
        public async Task<IActionResult> UpdateDecision(Guid workspaceId, Guid id, Guid decisionId, [FromBody] object dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _goalService.UpdateDecisionAsync(id, decisionId, userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
        public async Task<IActionResult> DeleteDecision(Guid workspaceId, Guid id, Guid decisionId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            await _goalService.DeleteDecisionAsync(id, decisionId, userId);
            return NoContent();
        }
    }
}
