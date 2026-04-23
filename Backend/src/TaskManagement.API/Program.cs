// Nhớ thêm thư viện này để dùng DbContext và SQL Server
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

using TaskManagement.Infrastructure.Data;

using TaskManagement.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 1. Mở tính năng Controllers (Chuẩn bị cho các API Login, Task...)
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("FixedWindow", httpContext =>
    {
        var userKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var partitionKey = !string.IsNullOrWhiteSpace(userKey)
            ? $"user:{userKey}"
            : $"ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

// Đăng ký Custom Services từ Extension Methods
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddWorkspaceServices();
builder.Services.AddAuditLogServices();

// 2. Khai báo Policy CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // Cho phép Vue.js gọi vào (các port dev server có thể khác nhau)
                          policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                      });
});

// 3. CẤU HÌNH CODE-FIRST (ENTITY FRAMEWORK CORE)
// Luôn dùng SQL Server để dữ liệu được lưu trữ vĩnh viễn (comment, notification, v.v.)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemoryFallback = false;

if (!string.IsNullOrWhiteSpace(defaultConnection) && builder.Environment.IsDevelopment())
{
    useInMemoryFallback = !CanConnectToSqlServer(defaultConnection);
    if (useInMemoryFallback)
    {
        Console.WriteLine("SQL Server is unavailable. Falling back to InMemory database for Development.");
    }
}

if (!string.IsNullOrWhiteSpace(defaultConnection) && !useInMemoryFallback)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(defaultConnection,
           sqlOptions => sqlOptions.EnableRetryOnFailure(
               maxRetryCount: 5,
               maxRetryDelay: TimeSpan.FromSeconds(30),
               errorNumbersToAdd: null)));
}
else
{
    // Fallback to InMemory chỉ khi không có connection string
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("DevInMemoryDb"));
}


var app = builder.Build();

// ---------------- CẤU HÌNH PIPELINE ----------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<TaskManagement.API.Middlewares.PerformanceMiddleware>();
app.UseMiddleware<TaskManagement.API.Middlewares.IpWhitelistMiddleware>();

// app.UseHttpsRedirection(); // Tắt HTTPS redirect để Axios có thể gọi vào HTTP 5136 mà ko bị CORS lỗi

// Middleware for Google OAuth popup support
app.Use(async (context, next) =>
{
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    await next();
});


// 4. KÍCH HOẠT CORS (Vị trí cực kỳ quan trọng, phải đứng trước Authorization)
app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseDefaultFiles(); // Phải gọi dòng này trước
app.UseStaticFiles();

// Serve uploaded files from /uploads
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// 5. Nối các endpoint vào Controllers
app.MapControllers();
app.MapHub<TaskManagement.API.Hubs.KanbanHub>("/kanban-hub");
app.MapHub<TaskManagement.API.Hubs.NotificationHub>("/notification-hub");

// TỰ ĐỘNG MIGRATE VÀ SEED DỮ LIỆU KHI STARTUP (PM: Vui lòng không xóa đoạn này)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    try 
    {
        // QUAN TRỌNG: Không xóa DB mỗi lần khởi động
        // await context.Database.EnsureDeletedAsync();
        // await context.Database.EnsureCreatedAsync();
        if (context.Database.IsRelational())
        {
            await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422145022_PlaneRenovation'
)
AND OBJECT_ID(N'dbo.AIPromptTemplates', N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.Organizations', N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.Permissions', N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.ProjectTemplates', N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.SystemSettings', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.Users', N'AvatarUrl') IS NOT NULL
AND COL_LENGTH(N'dbo.Users', N'CoverUrl') IS NOT NULL
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260422145022_PlaneRenovation', N'10.0.5');
END;
");

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception migrationEx)
            {
                Console.WriteLine("Migration warning/error, continuing with schema guard: " + migrationEx.Message);
            }

            await context.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('dbo.TaskDrafts', 'ProjectId') IS NULL
BEGIN
    ALTER TABLE dbo.TaskDrafts ADD ProjectId uniqueidentifier NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskDrafts_UserId_ProjectId_UpdatedAt' AND object_id = OBJECT_ID('dbo.TaskDrafts'))
BEGIN
    EXEC('CREATE INDEX IX_TaskDrafts_UserId_ProjectId_UpdatedAt ON dbo.TaskDrafts(UserId, ProjectId, UpdatedAt);');
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskDrafts_UserId_UpdatedAt' AND object_id = OBJECT_ID('dbo.TaskDrafts'))
BEGIN
    CREATE INDEX IX_TaskDrafts_UserId_UpdatedAt ON dbo.TaskDrafts(UserId, UpdatedAt);
END;
IF COL_LENGTH('dbo.Pages', 'IsPrivate') IS NULL
BEGIN
    ALTER TABLE dbo.Pages ADD IsPrivate bit NOT NULL CONSTRAINT DF_Pages_IsPrivate DEFAULT(0);
END;
IF COL_LENGTH('dbo.Pages', 'IsStarred') IS NULL
BEGIN
    ALTER TABLE dbo.Pages ADD IsStarred bit NOT NULL CONSTRAINT DF_Pages_IsStarred DEFAULT(0);
END;
IF COL_LENGTH('dbo.Users', 'AvatarUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD AvatarUrl nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Users', 'CoverUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD CoverUrl nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.SystemSettings', 'Description') IS NULL
BEGIN
    ALTER TABLE dbo.SystemSettings ADD Description nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.SystemSettings', 'LastModifiedAt') IS NULL
BEGIN
    ALTER TABLE dbo.SystemSettings ADD LastModifiedAt datetime2 NOT NULL CONSTRAINT DF_SystemSettings_LastModifiedAt DEFAULT SYSUTCDATETIME();
END;
IF COL_LENGTH('dbo.TaskStatuses', 'ColorCode') IS NULL
BEGIN
    ALTER TABLE dbo.TaskStatuses ADD ColorCode nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.AIPromptTemplates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIPromptTemplates (
        Id uniqueidentifier NOT NULL,
        Code nvarchar(max) NOT NULL,
        TemplateContent nvarchar(max) NOT NULL,
        IsActive bit NOT NULL,
        CONSTRAINT PK_AIPromptTemplates PRIMARY KEY (Id)
    );
END;
IF OBJECT_ID('dbo.TaskSubscribers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskSubscribers (
        WorkTaskId uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        SubscribedAt datetime2 NOT NULL CONSTRAINT DF_TaskSubscribers_SubscribedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TaskSubscribers PRIMARY KEY (WorkTaskId, UserId),
        CONSTRAINT FK_TaskSubscribers_WorkTasks_WorkTaskId FOREIGN KEY (WorkTaskId) REFERENCES dbo.WorkTasks(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskSubscribers_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskSubscribers_UserId' AND object_id = OBJECT_ID('dbo.TaskSubscribers'))
BEGIN
    CREATE INDEX IX_TaskSubscribers_UserId ON dbo.TaskSubscribers(UserId);
END;
IF COL_LENGTH('dbo.TaskDrafts', 'ProjectId') IS NOT NULL
BEGIN
    UPDATE td
    SET ProjectId = TRY_CONVERT(uniqueidentifier, JSON_VALUE(td.PayloadJson, '$.projectId'))
    FROM dbo.TaskDrafts td
    WHERE td.ProjectId IS NULL
      AND ISJSON(td.PayloadJson) = 1
      AND JSON_VALUE(td.PayloadJson, '$.projectId') IS NOT NULL;
END;
");
        }
        await TaskManagement.Infrastructure.Data.DataSeeder.SeedMockDataAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi khi Migrate/Seed: " + ex);
    }
}

app.MapFallbackToFile("index.html");
app.Run();

static bool CanConnectToSqlServer(string connectionString)
{
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 3
        };

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();
        return true;
    }
    catch
    {
        return false;
    }
}
