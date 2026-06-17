using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/kudos")]
    [Authorize]
    public class KudosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KudosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("team/{departmentId}")]
        public async Task<IActionResult> GetByTeam(Guid departmentId)
        {
            var kudos = await _context.Kudos
                .Include(k => k.Sender)
                .Where(k => k.DepartmentId == departmentId)
                .OrderByDescending(k => k.CreatedAt)
                .Select(k => new
                {
                    id = k.Id,
                    message = k.Message,
                    sender = k.Sender.FullName ?? k.Sender.Email,
                    icon = k.Icon ?? "🌟",
                    createdAt = k.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Success(kudos));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKudoDto dto)
        {
            // Assuming we get UserId from claims. For MVP, we pass it or mock it.
            var senderId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            if (senderId == Guid.Empty) return Unauthorized();

            var kudo = new Kudo
            {
                SenderId = senderId,
                DepartmentId = dto.DepartmentId,
                Message = dto.Message,
                Icon = dto.Icon,
                CreatedAt = DateTime.UtcNow
            };

            _context.Kudos.Add(kudo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success(new { id = kudo.Id }, "Đã gửi lời khen ngợi."));
        }
    }

    public class CreateKudoDto
    {
        public Guid DepartmentId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Icon { get; set; }
    }
}
