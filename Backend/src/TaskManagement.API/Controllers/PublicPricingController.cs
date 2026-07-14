using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/public/pricing")]
    public class PublicPricingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicPricingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetPricing()
        {
            var plans = await _context.AiPricingPlans
                .AsNoTracking()
                .OrderBy(plan => plan.Code == "free" ? 0 : plan.Code == "team" ? 1 : plan.Code == "business" ? 2 : 9)
                .ThenBy(plan => plan.Code)
                .Select(plan => new
                {
                    id = plan.Code,
                    plan.Name,
                    plan.MonthlyPriceVnd,
                    priceStatus = plan.PricingStatus,
                    plan.PerUser,
                    plan.IncludedUsers,
                    plan.IncludedAiCredits,
                    plan.ExtraAiCreditsEnabled,
                    plan.FeaturesJson,
                    plan.UpdatedAt
                })
                .ToListAsync();

            var aiCreditRules = await _context.AiCreditRules
                .AsNoTracking()
                .Where(rule => rule.IsActive)
                .OrderBy(rule => rule.ActionType)
                .Select(rule => new
                {
                    rule.ActionType,
                    rule.EstimatedCredits,
                    billable = true,
                    rule.Disclaimer,
                    rule.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                statusCode = 200,
                data = new
                {
                    currency = "VND",
                    billingPeriod = "month",
                    plans = plans.Select(plan => new
                    {
                        plan.id,
                        plan.Name,
                        plan.MonthlyPriceVnd,
                        plan.priceStatus,
                        plan.PerUser,
                        plan.IncludedUsers,
                        plan.IncludedAiCredits,
                        plan.ExtraAiCreditsEnabled,
                        features = SafeDeserializeStringArray(plan.FeaturesJson),
                        plan.UpdatedAt
                    }),
                    aiCreditRules,
                    source = "database",
                    disclaimer = "Paid plan prices remain null until Product Owner approval. AI usage is read from server-side ledger/token logs."
                }
            });
        }

        private static string[] SafeDeserializeStringArray(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<string[]>(raw) ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
