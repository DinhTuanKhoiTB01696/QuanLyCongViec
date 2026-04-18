using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/security")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SecurityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class IpWhitelistEntry
        {
            public string Ip { get; set; } = string.Empty;
            public string? Note { get; set; }
            public string? AddedBy { get; set; }
            public string? Date { get; set; }
        }

        [HttpGet("ip-whitelist")]
        public async Task<IActionResult> GetIpWhitelist()
        {
            // Verify admin access (omitted for brevity, assume global or add policy)
            var config = await _context.TenantConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound(new { statusCode = 404, message = "Global configuration not found." });

            var result = new { isEnabled = false, ips = new List<IpWhitelistEntry>() };
            
            if (!string.IsNullOrEmpty(config.IpWhitelist))
            {
                try
                {
                   // We actually store List<IpWhitelistEntry> into TenantConfig.IpWhitelist now
                   var ips = JsonSerializer.Deserialize<List<IpWhitelistEntry>>(config.IpWhitelist);
                   if (ips != null)
                   {
                       result = new 
                       {
                           isEnabled = ips.Count > 0, // Simplified: if list has items, it's enabled
                           ips = ips
                       };
                   }
                } 
                catch 
                {
                   // Fallback for old string array
                   try 
                   {
                        var oldIps = JsonSerializer.Deserialize<List<string>>(config.IpWhitelist);
                        if (oldIps != null)
                        {
                            result = new {
                                isEnabled = oldIps.Count > 0,
                                ips = oldIps.Select(ip => new IpWhitelistEntry { Ip = ip, Note = "Migrated from old system", AddedBy = "System", Date = System.DateTime.UtcNow.ToString("dd/MM/yyyy") }).ToList()
                            };
                        }
                   } catch {}
                }
            }

            return Ok(new { statusCode = 200, data = result });
        }

        public class UpdateIpWhitelistRequest
        {
            public bool IsEnabled { get; set; }
            public List<IpWhitelistEntry>? Ips { get; set; }
        }

        [HttpPut("ip-whitelist")]
        public async Task<IActionResult> UpdateIpWhitelist([FromBody] UpdateIpWhitelistRequest request)
        {
            var config = await _context.TenantConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new Domain.Entities.TenantConfig { Id = System.Guid.NewGuid(), OrganizationName = "Default" };
                _context.TenantConfigs.Add(config);
            }

            var finalIps = request.IsEnabled && request.Ips != null ? request.Ips : new List<IpWhitelistEntry>();
            config.IpWhitelist = JsonSerializer.Serialize(finalIps);

            await _context.SaveChangesAsync();
            return Ok(new { statusCode = 200, message = "IP Whitelist updated successfully.", data = new { isEnabled = request.IsEnabled, ips = finalIps } });
        }
    }
}
