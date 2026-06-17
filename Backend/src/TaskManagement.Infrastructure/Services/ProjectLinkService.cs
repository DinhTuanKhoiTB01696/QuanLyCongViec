using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class ProjectLinkService : IProjectLinkService
    {
        private readonly ApplicationDbContext _context;

        public ProjectLinkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllLinksAsync(Guid projectId)
        {
            return await _context.ProjectLinks
                .Where(p => p.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<object> CreateLinkAsync(Guid creatorId, Guid projectId, object dto)
        {
            var link = new ProjectLink
            {
                Id = Guid.NewGuid(),
                CreatorId = creatorId,
                ProjectId = projectId,
                LinkedType = "Goal", // Placeholder
                CreatedAt = DateTime.UtcNow
            };
            _context.ProjectLinks.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task DeleteLinkAsync(Guid id)
        {
            var link = await _context.ProjectLinks.FindAsync(id);
            if (link != null)
            {
                _context.ProjectLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
        }
    }
}
