using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.Roles
                .Select(r => new { r.Id, r.Name, r.Description, r.Badge })
                .ToListAsync();
            return Ok(new { statusCode = 200, data = roles });
        }

        // POST: api/roles (create or clone)
        [HttpPost]
        public async Task<IActionResult> CreateOrClone([FromBody] RoleCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { statusCode = 400, message = "Invalid role data." });

            // If SourceId is provided, perform clone
            if (dto.SourceId.HasValue)
            {
                var source = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.SourceId.Value);
                if (source == null)
                    return NotFound(new { statusCode = 404, message = "Source role not found." });

                var clone = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description ?? source.Description,
                    Badge = dto.Badge ?? source.Badge,
                    RolePermissions = dto.CopyMembers ? source.RolePermissions.Select(rp => new RolePermission { PermissionId = rp.PermissionId }).ToList() : new List<RolePermission>()
                };
                _context.Roles.Add(clone);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = clone.Id }, new { statusCode = 201, data = clone });
            }
            else
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Badge = dto.Badge
                };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, new { statusCode = 201, data = role });
            }
        }

        // GET: api/roles/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new { r.Id, r.Name, r.Description, r.Badge })
                .FirstOrDefaultAsync();
            if (role == null)
                return NotFound(new { statusCode = 404, message = "Role not found." });
            return Ok(new { statusCode = 200, data = role });
        }
    }

    public class RoleCreateDto
    {
        public Guid? SourceId { get; set; } // for cloning
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Badge { get; set; }
        public bool CopyMembers { get; set; } = false; // copy permissions
    }
}
