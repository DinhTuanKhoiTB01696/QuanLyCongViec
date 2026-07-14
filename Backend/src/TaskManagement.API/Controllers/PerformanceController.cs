using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.DTOs.Common;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/performance")]
    [Authorize]
    public class PerformanceController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public PerformanceController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet("summary")]
        public IActionResult Summary()
        {
            var metrics = _cache.TryGetValue("ApiPerformanceMetrics", out List<long>? cached) && cached != null
                ? cached.OrderBy(item => item).ToList()
                : new List<long>();

            var count = metrics.Count;
            long Percentile(double percentile)
            {
                if (count == 0) return 0;
                var index = (int)Math.Ceiling(percentile * count) - 1;
                return metrics[Math.Clamp(index, 0, count - 1)];
            }

            return Ok(ApiResponse<object>.Success(new
            {
                sampleWindow = 100,
                count,
                averageMs = count == 0 ? 0 : Math.Round(metrics.Average(), 2),
                p50Ms = Percentile(0.50),
                p95Ms = Percentile(0.95),
                maxMs = count == 0 ? 0 : metrics[^1],
                status = count == 0 ? "NoSamples" : Percentile(0.95) <= 1000 ? "Healthy" : "NeedsAttention"
            }));
        }
    }
}
