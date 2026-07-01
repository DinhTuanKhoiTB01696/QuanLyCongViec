using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký Application Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProjectMemberService, ProjectMemberService>();
            services.AddScoped<IWorkTaskService, WorkTaskService>();
            services.AddScoped<IGamificationService, GamificationService>();
            services.AddScoped<IAiIntegrationService, AiIntegrationService>();
            services.AddHttpClient<IAiService, GeminiAiService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Cấu hình JWT Authentication
            var jwtConfig = configuration.GetSection("Jwt");
            var secretKey = jwtConfig["SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey is missing");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Nên đổi sang true trên Prod
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Hết hạn là hết hạn ngay
                };
            });

            return services;
        }

        /// <summary>
        /// Module 5: Workspace & Agile Planning — DI Registration
        /// </summary>
        public static IServiceCollection AddWorkspaceServices(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ISprintService, SprintService>();
            
            // Phase 1: SprintA Alignment Entities
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<IProjectLinkService, ProjectLinkService>();
            services.AddScoped<IStarredItemService, StarredItemService>();
            services.AddScoped<IFollowerService, FollowerService>();
            
            return services;
        }

        /// <summary>
        /// Module 6: System Audit & Logging — DI Registration
        /// </summary>
        public static IServiceCollection AddAuditLogServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuditLogQueue, AuditLogQueue>();
            services.AddHostedService<AuditLogWorker>();
            services.AddScoped<ISignalRClientNotifier, TaskManagement.API.Services.SignalRClientNotifier>();
            services.AddScoped<ISiteAuditService, SiteAuditService>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}

