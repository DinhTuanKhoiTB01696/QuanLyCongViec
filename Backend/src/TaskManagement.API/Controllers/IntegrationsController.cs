using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Infrastructure;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/integrations")]
    [Authorize]
    public class IntegrationsController : ControllerBase
    {
        private const string GoogleCalendarProvider = "google-calendar";
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public IntegrationsController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetIntegrations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var accounts = await _context.IntegrationAccounts
                .AsNoTracking()
                .Where(account => account.UserId == userId.Value && account.IsActive)
                .ToListAsync();

            var google = accounts.FirstOrDefault(account => account.Provider == GoogleCalendarProvider);
            var histories = await _context.SyncHistories
                .AsNoTracking()
                .Where(history => history.UserId == userId.Value)
                .OrderByDescending(history => history.StartedAt)
                .Take(10)
                .Select(history => new
                {
                    history.Id,
                    history.Provider,
                    history.Status,
                    history.ItemsImported,
                    history.Message,
                    history.StartedAt,
                    history.CompletedAt
                })
                .ToListAsync();

            return Ok(new
            {
                statusCode = 200,
                data = new
                {
                    providers = new object[]
                    {
                        new
                        {
                            id = google?.Id,
                            provider = GoogleCalendarProvider,
                            name = "Google Calendar",
                            source = "calendar",
                            status = google == null ? "not_connected" : "connected",
                            accountEmail = google?.AccountEmail,
                            lastSyncedAt = google?.LastSyncedAt,
                            connectedAt = google?.CreatedAt,
                            supportsConnect = true,
                            supportsSync = true
                        },
                        new
                        {
                            id = (Guid?)null,
                            provider = "gmail",
                            name = "Gmail",
                            source = "email",
                            status = "coming_soon",
                            accountEmail = (string?)null,
                            lastSyncedAt = (DateTime?)null,
                            connectedAt = (DateTime?)null,
                            supportsConnect = false,
                            supportsSync = false
                        },
                        new
                        {
                            id = (Guid?)null,
                            provider = "slack",
                            name = "Slack",
                            source = "slack",
                            status = "coming_soon",
                            accountEmail = (string?)null,
                            lastSyncedAt = (DateTime?)null,
                            connectedAt = (DateTime?)null,
                            supportsConnect = false,
                            supportsSync = false
                        }
                    },
                    syncHistory = histories
                }
            });
        }

        [HttpGet("google-calendar/connect")]
        public IActionResult ConnectGoogleCalendar()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var clientId = _configuration["IntegrationOAuth:GoogleCalendar:ClientId"];
            var clientSecret = _configuration["IntegrationOAuth:GoogleCalendar:ClientSecret"];
            var redirectUri = _configuration["IntegrationOAuth:GoogleCalendar:RedirectUri"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirectUri))
            {
                return BadRequest(new { message = "Google Calendar OAuth chưa được cấu hình đầy đủ" });
            }

            var stateJson = JsonSerializer.Serialize(new
            {
                userId = userId.Value,
                provider = GoogleCalendarProvider,
                nonce = Guid.NewGuid(),
                createdAt = DateTime.UtcNow
            });
            var state = Base64UrlEncode(Encoding.UTF8.GetBytes(stateJson));
            var scope = Uri.EscapeDataString("openid email profile https://www.googleapis.com/auth/calendar.readonly");
            var url =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                "&response_type=code" +
                $"&scope={scope}" +
                "&access_type=offline" +
                "&prompt=consent" +
                $"&state={Uri.EscapeDataString(state)}";

            return Ok(new { statusCode = 200, data = new { authorizationUrl = url } });
        }

        [HttpGet("google-calendar/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCalendarCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error, [FromQuery(Name = "error_description")] string? errorDescription)
        {
            var frontendUrl = GetFrontendIntegrationUrl();
            if (!string.IsNullOrWhiteSpace(error))
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", errorDescription ?? error));
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", "Thiếu mã xác thực từ Google"));
            }

            var statePayload = DecodeState(state);
            if (statePayload == null || statePayload.UserId == Guid.Empty || statePayload.Provider != GoogleCalendarProvider)
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", "OAuth state không hợp lệ"));
            }

            var userId = statePayload.UserId;
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var clientId = _configuration["IntegrationOAuth:GoogleCalendar:ClientId"];
            var clientSecret = _configuration["IntegrationOAuth:GoogleCalendar:ClientSecret"];
            var redirectUri = _configuration["IntegrationOAuth:GoogleCalendar:RedirectUri"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirectUri))
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", "Google Calendar OAuth chưa được cấu hình đầy đủ"));
            }

            var client = _httpClientFactory.CreateClient();
            var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            }));

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", "Không đổi được Google OAuth token"));
            }

            var token = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenJson);
            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return Redirect(BuildFrontendRedirect(frontendUrl, "error", "Google không trả access token hợp lệ"));
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var userInfoResponse = await client.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");
            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = userInfoResponse.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<GoogleUserInfoResponse>(userInfoJson)
                : null;

            var account = await _context.IntegrationAccounts
                .FirstOrDefaultAsync(item => item.UserId == userId && item.Provider == GoogleCalendarProvider);

            if (account == null)
            {
                account = new IntegrationAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Provider = GoogleCalendarProvider,
                    CreatedAt = DateTime.UtcNow
                };
                _context.IntegrationAccounts.Add(account);
            }

            account.AccountEmail = userInfo?.Email ?? account.AccountEmail;
            account.ExternalAccountId = userInfo?.Sub ?? account.ExternalAccountId;
            account.AccessToken = token.AccessToken;
            if (!string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                account.RefreshToken = token.RefreshToken;
            }
            account.AccessTokenExpiresAt = token.ExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60) : null;
            account.Scopes = token.Scope ?? string.Empty;
            account.IsActive = true;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Redirect(BuildFrontendRedirect(frontendUrl, "success", null));
        }

        [HttpPost("google-calendar/sync")]
        public async Task<IActionResult> SyncGoogleCalendar()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var account = await _context.IntegrationAccounts
                .FirstOrDefaultAsync(item => item.UserId == userId.Value && item.Provider == GoogleCalendarProvider && item.IsActive);

            if (account == null)
            {
                return BadRequest(new { message = "Google Calendar chưa kết nối" });
            }

            var history = new SyncHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                IntegrationAccountId = account.Id,
                Provider = GoogleCalendarProvider,
                Status = "running",
                StartedAt = DateTime.UtcNow
            };
            _context.SyncHistories.Add(history);
            await _context.SaveChangesAsync();

            try
            {
                await EnsureGoogleAccessTokenAsync(account);

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);
                var timeMin = Uri.EscapeDataString(DateTime.UtcNow.AddDays(-7).ToString("O"));
                var timeMax = Uri.EscapeDataString(DateTime.UtcNow.AddDays(30).ToString("O"));
                var eventsResponse = await client.GetAsync(
                    $"https://www.googleapis.com/calendar/v3/calendars/primary/events?singleEvents=true&orderBy=startTime&timeMin={timeMin}&timeMax={timeMax}");

                var eventsJson = await eventsResponse.Content.ReadAsStringAsync();
                if (!eventsResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(eventsJson);
                }

                var calendarEvents = JsonSerializer.Deserialize<GoogleCalendarEventsResponse>(eventsJson);
                var imported = 0;
                foreach (var calendarEvent in calendarEvents?.Items ?? new List<GoogleCalendarEvent>())
                {
                    if (string.IsNullOrWhiteSpace(calendarEvent.Id)) continue;

                    var externalId = calendarEvent.Id;
                    var item = await _context.InboxItems
                        .FirstOrDefaultAsync(existing => existing.UserId == userId.Value
                            && existing.Provider == GoogleCalendarProvider
                            && existing.ExternalId == externalId);

                    if (item == null)
                    {
                        item = new InboxItem
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId.Value,
                            IntegrationAccountId = account.Id,
                            Source = "calendar",
                            Provider = GoogleCalendarProvider,
                            ExternalId = externalId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InboxItems.Add(item);
                    }

                    item.Title = string.IsNullOrWhiteSpace(calendarEvent.Summary)
                        ? "Sự kiện Google Calendar"
                        : calendarEvent.Summary;
                    item.Content = calendarEvent.Description;
                    item.Location = calendarEvent.Location;
                    item.StartsAt = ParseGoogleDate(calendarEvent.Start);
                    item.EndsAt = ParseGoogleDate(calendarEvent.End);
                    item.UpdatedAt = DateTime.UtcNow;
                    imported += 1;
                }

                account.LastSyncedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;
                history.Status = "success";
                history.ItemsImported = imported;
                history.Message = $"Đã đồng bộ {imported} sự kiện Google Calendar";
                history.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { statusCode = 200, data = new { imported, account.LastSyncedAt } });
            }
            catch (Exception ex)
            {
                history.Status = "error";
                history.Message = ex.Message;
                history.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return StatusCode(502, new { message = "Không đồng bộ được Google Calendar", detail = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Disconnect(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await IntegrationSchemaGuard.EnsureCreatedAsync(_context);

            var account = await _context.IntegrationAccounts
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId.Value);

            if (account == null) return NotFound(new { message = "Không tìm thấy kết nối" });

            account.IsActive = false;
            account.AccessToken = string.Empty;
            account.RefreshToken = null;
            account.AccessTokenExpiresAt = null;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Đã ngắt kết nối" });
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        private async Task EnsureGoogleAccessTokenAsync(IntegrationAccount account)
        {
            if (!string.IsNullOrWhiteSpace(account.AccessToken)
                && (!account.AccessTokenExpiresAt.HasValue || account.AccessTokenExpiresAt.Value > DateTime.UtcNow))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(account.RefreshToken))
            {
                throw new InvalidOperationException("Google token đã hết hạn, vui lòng kết nối lại");
            }

            var clientId = _configuration["IntegrationOAuth:GoogleCalendar:ClientId"];
            var clientSecret = _configuration["IntegrationOAuth:GoogleCalendar:ClientSecret"];
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("Google Calendar OAuth chưa được cấu hình đầy đủ");
            }

            var client = _httpClientFactory.CreateClient();
            var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = account.RefreshToken,
                ["grant_type"] = "refresh_token"
            }));

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(tokenJson);
            }

            var token = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenJson);
            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("Google không trả access token hợp lệ");
            }

            account.AccessToken = token.AccessToken;
            account.AccessTokenExpiresAt = token.ExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60) : null;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static DateTime? ParseGoogleDate(GoogleDateTime? value)
        {
            if (value == null) return null;
            if (!string.IsNullOrWhiteSpace(value.DateTime) && DateTimeOffset.TryParse(value.DateTime, out var dto))
            {
                return dto.UtcDateTime;
            }
            if (!string.IsNullOrWhiteSpace(value.Date) && DateTime.TryParse(value.Date, out var date))
            {
                return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            }
            return null;
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private string GetFrontendIntegrationUrl()
        {
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = "http://localhost:5173";
            }

            return $"{frontendBaseUrl}/integrations";
        }

        private static string BuildFrontendRedirect(string frontendUrl, string status, string? message)
        {
            var query = new List<string>
            {
                "provider=google-calendar",
                status == "success" ? "connected=success" : "connected=error"
            };

            if (!string.IsNullOrWhiteSpace(message))
            {
                query.Add($"message={Uri.EscapeDataString(message)}");
            }

            return $"{frontendUrl}?{string.Join("&", query)}";
        }

        private static OAuthState? DecodeState(string state)
        {
            try
            {
                var padded = state.Replace('-', '+').Replace('_', '/');
                padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
                return JsonSerializer.Deserialize<OAuthState>(Encoding.UTF8.GetString(Convert.FromBase64String(padded)));
            }
            catch
            {
                return null;
            }
        }

        private sealed class OAuthState
        {
            [JsonPropertyName("userId")]
            public Guid UserId { get; set; }

            [JsonPropertyName("provider")]
            public string Provider { get; set; } = string.Empty;
        }

        private sealed class GoogleTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public string? Scope { get; set; }
        }

        private sealed class GoogleUserInfoResponse
        {
            [JsonPropertyName("sub")]
            public string? Sub { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }
        }

        private sealed class GoogleCalendarEventsResponse
        {
            [JsonPropertyName("items")]
            public List<GoogleCalendarEvent> Items { get; set; } = new();
        }

        private sealed class GoogleCalendarEvent
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("summary")]
            public string? Summary { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("location")]
            public string? Location { get; set; }

            [JsonPropertyName("start")]
            public GoogleDateTime? Start { get; set; }

            [JsonPropertyName("end")]
            public GoogleDateTime? End { get; set; }
        }

        private sealed class GoogleDateTime
        {
            [JsonPropertyName("dateTime")]
            public string? DateTime { get; set; }

            [JsonPropertyName("date")]
            public string? Date { get; set; }
        }
    }
}
