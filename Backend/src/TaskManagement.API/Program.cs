// Nhớ thêm thư viện này để dùng DbContext và SQL Server
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(defaultConnection,
           sqlOptions => sqlOptions.EnableRetryOnFailure(
               maxRetryCount: 5,
               maxRetryDelay: TimeSpan.FromSeconds(30),
               errorNumbersToAdd: null))
           .ConfigureWarnings(warnings =>
               warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
}
else
{
    // Fallback to InMemory chỉ khi không có connection string
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("DevInMemoryDb")
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning);
            }));
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
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

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
IF OBJECT_ID('dbo.TeamGoals', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeamGoals (
        Id uniqueidentifier NOT NULL,
        DepartmentId uniqueidentifier NOT NULL,
        GoalId uniqueidentifier NOT NULL,
        CreatedByUserId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_TeamGoals_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TeamGoals PRIMARY KEY (Id),
        CONSTRAINT FK_TeamGoals_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TeamGoals_Goals_GoalId FOREIGN KEY (GoalId) REFERENCES dbo.Goals(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TeamGoals_Users_CreatedByUserId FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamGoals_DepartmentId_GoalId' AND object_id = OBJECT_ID('dbo.TeamGoals'))
BEGIN
    CREATE UNIQUE INDEX IX_TeamGoals_DepartmentId_GoalId ON dbo.TeamGoals(DepartmentId, GoalId);
END;
IF OBJECT_ID('dbo.RecentViews', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecentViews (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        EntityType nvarchar(64) NOT NULL,
        EntityId uniqueidentifier NOT NULL,
        Title nvarchar(512) NOT NULL,
        Subtitle nvarchar(512) NULL,
        Url nvarchar(1024) NULL,
        Icon nvarchar(128) NULL,
        ViewedAt datetime2 NOT NULL CONSTRAINT DF_RecentViews_ViewedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_RecentViews PRIMARY KEY (Id),
        CONSTRAINT FK_RecentViews_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RecentViews_UserId_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.RecentViews'))
BEGIN
    CREATE UNIQUE INDEX IX_RecentViews_UserId_EntityType_EntityId ON dbo.RecentViews(UserId, EntityType, EntityId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RecentViews_UserId_ViewedAt' AND object_id = OBJECT_ID('dbo.RecentViews'))
BEGIN
    CREATE INDEX IX_RecentViews_UserId_ViewedAt ON dbo.RecentViews(UserId, ViewedAt DESC);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskSubscribers_UserId' AND object_id = OBJECT_ID('dbo.TaskSubscribers'))
BEGIN
    CREATE INDEX IX_TaskSubscribers_UserId ON dbo.TaskSubscribers(UserId);
END;
IF OBJECT_ID('dbo.GoalUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.GoalUpdates', 'OldStatus') IS NULL
BEGIN
    ALTER TABLE dbo.GoalUpdates ADD OldStatus nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.GoalUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.GoalUpdates', 'NewStatus') IS NULL
BEGIN
    ALTER TABLE dbo.GoalUpdates ADD NewStatus nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.GoalUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.GoalUpdates', 'OldProgress') IS NULL
BEGIN
    ALTER TABLE dbo.GoalUpdates ADD OldProgress int NULL;
END;
IF OBJECT_ID('dbo.GoalUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.GoalUpdates', 'NewProgress') IS NULL
BEGIN
    ALTER TABLE dbo.GoalUpdates ADD NewProgress int NULL;
END;
IF OBJECT_ID('dbo.ProjectUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.ProjectUpdates', 'OldStatus') IS NULL
BEGIN
    ALTER TABLE dbo.ProjectUpdates ADD OldStatus nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.ProjectUpdates', 'U') IS NOT NULL AND COL_LENGTH('dbo.ProjectUpdates', 'NewStatus') IS NULL
BEGIN
    ALTER TABLE dbo.ProjectUpdates ADD NewStatus nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.EntityFollowers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntityFollowers (
        Id uniqueidentifier NOT NULL,
        EntityId uniqueidentifier NOT NULL,
        EntityType nvarchar(128) NOT NULL,
        UserId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_EntityFollowers_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EntityFollowers PRIMARY KEY (Id),
        CONSTRAINT FK_EntityFollowers_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF OBJECT_ID('dbo.EntityFollowers', 'U') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM sys.columns
       WHERE object_id = OBJECT_ID('dbo.EntityFollowers')
         AND name = 'EntityType'
         AND max_length = -1
   )
BEGIN
    UPDATE dbo.EntityFollowers SET EntityType = LEFT(EntityType, 128) WHERE LEN(EntityType) > 128;
    ALTER TABLE dbo.EntityFollowers ALTER COLUMN EntityType nvarchar(128) NOT NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EntityFollowers_UserId' AND object_id = OBJECT_ID('dbo.EntityFollowers'))
BEGIN
    CREATE INDEX IX_EntityFollowers_UserId ON dbo.EntityFollowers(UserId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EntityFollowers_UserId_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.EntityFollowers'))
BEGIN
    CREATE UNIQUE INDEX IX_EntityFollowers_UserId_EntityType_EntityId ON dbo.EntityFollowers(UserId, EntityType, EntityId);
END;
IF OBJECT_ID('dbo.ProjectUpdates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectUpdates (
        Id uniqueidentifier NOT NULL,
        ProjectId uniqueidentifier NOT NULL,
        Content nvarchar(max) NOT NULL,
        Status nvarchar(max) NOT NULL,
        OldStatus nvarchar(max) NULL,
        NewStatus nvarchar(max) NULL,
        CreatorId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_ProjectUpdates_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ProjectUpdates PRIMARY KEY (Id),
        CONSTRAINT FK_ProjectUpdates_Projects_ProjectId FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectUpdates_Users_CreatorId FOREIGN KEY (CreatorId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectUpdates_ProjectId' AND object_id = OBJECT_ID('dbo.ProjectUpdates'))
BEGIN
    CREATE INDEX IX_ProjectUpdates_ProjectId ON dbo.ProjectUpdates(ProjectId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectUpdates_CreatorId' AND object_id = OBJECT_ID('dbo.ProjectUpdates'))
BEGIN
    CREATE INDEX IX_ProjectUpdates_CreatorId ON dbo.ProjectUpdates(CreatorId);
END;
IF OBJECT_ID('dbo.ProjectLessons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectLessons (
        Id uniqueidentifier NOT NULL,
        ProjectId uniqueidentifier NOT NULL,
        Text nvarchar(max) NOT NULL,
        CreatorId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_ProjectLessons_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ProjectLessons PRIMARY KEY (Id),
        CONSTRAINT FK_ProjectLessons_Projects_ProjectId FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectLessons_Users_CreatorId FOREIGN KEY (CreatorId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectLessons_ProjectId' AND object_id = OBJECT_ID('dbo.ProjectLessons'))
BEGIN
    CREATE INDEX IX_ProjectLessons_ProjectId ON dbo.ProjectLessons(ProjectId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectLessons_CreatorId' AND object_id = OBJECT_ID('dbo.ProjectLessons'))
BEGIN
    CREATE INDEX IX_ProjectLessons_CreatorId ON dbo.ProjectLessons(CreatorId);
END;
IF OBJECT_ID('dbo.ProjectRisks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectRisks (
        Id uniqueidentifier NOT NULL,
        ProjectId uniqueidentifier NOT NULL,
        Text nvarchar(max) NOT NULL,
        Severity nvarchar(max) NOT NULL,
        CreatorId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_ProjectRisks_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ProjectRisks PRIMARY KEY (Id),
        CONSTRAINT FK_ProjectRisks_Projects_ProjectId FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectRisks_Users_CreatorId FOREIGN KEY (CreatorId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectRisks_ProjectId' AND object_id = OBJECT_ID('dbo.ProjectRisks'))
BEGIN
    CREATE INDEX IX_ProjectRisks_ProjectId ON dbo.ProjectRisks(ProjectId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectRisks_CreatorId' AND object_id = OBJECT_ID('dbo.ProjectRisks'))
BEGIN
    CREATE INDEX IX_ProjectRisks_CreatorId ON dbo.ProjectRisks(CreatorId);
END;
IF OBJECT_ID('dbo.ProjectDecisions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectDecisions (
        Id uniqueidentifier NOT NULL,
        ProjectId uniqueidentifier NOT NULL,
        Text nvarchar(max) NOT NULL,
        CreatorId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_ProjectDecisions_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ProjectDecisions PRIMARY KEY (Id),
        CONSTRAINT FK_ProjectDecisions_Projects_ProjectId FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectDecisions_Users_CreatorId FOREIGN KEY (CreatorId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectDecisions_ProjectId' AND object_id = OBJECT_ID('dbo.ProjectDecisions'))
BEGIN
    CREATE INDEX IX_ProjectDecisions_ProjectId ON dbo.ProjectDecisions(ProjectId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProjectDecisions_CreatorId' AND object_id = OBJECT_ID('dbo.ProjectDecisions'))
BEGIN
    CREATE INDEX IX_ProjectDecisions_CreatorId ON dbo.ProjectDecisions(CreatorId);
END;
IF COL_LENGTH('dbo.TaskDrafts', 'ProjectId') IS NOT NULL
BEGIN
    EXEC('
        UPDATE td
        SET ProjectId = TRY_CONVERT(uniqueidentifier, JSON_VALUE(td.PayloadJson, ''$.projectId''))
        FROM dbo.TaskDrafts td
        WHERE td.ProjectId IS NULL
          AND ISJSON(td.PayloadJson) = 1
          AND JSON_VALUE(td.PayloadJson, ''$.projectId'') IS NOT NULL;
    ');
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
