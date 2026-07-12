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
builder.Services.AddHttpClient();

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
                          var allowedOrigins = builder.Environment.IsDevelopment()
                              ? new[] { "http://localhost:5173", "http://localhost:4173" }
                              : builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                          policy.WithOrigins(allowedOrigins)
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
    WHERE [MigrationId] = N'20260630092237_PlaneRenovation'
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
    VALUES (N'20260630092237_PlaneRenovation', N'10.0.5');
END;
");

            try
            {
                // Legacy databases may contain StarredItems.ItemType values that are
                // outside the supported domain before the check constraint migration.
                await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.StarredItems', 'U') IS NOT NULL
BEGIN
    DELETE FROM dbo.StarredItems
    WHERE ItemType NOT IN ('Goal', 'Project', 'Team', 'User');
END;");

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
IF COL_LENGTH('dbo.Users', 'Bio') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Bio nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Users', 'JobTitle') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD JobTitle nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Users', 'Location') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Location nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Users', 'Timezone') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Timezone nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Projects', 'Why') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD Why nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Projects', 'SuccessCriteria') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD SuccessCriteria nvarchar(max) NULL;
END;
IF COL_LENGTH('dbo.Projects', 'CloseDate') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD CloseDate datetime2 NULL;
END;
IF COL_LENGTH('dbo.Projects', 'TrackedLinkUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD TrackedLinkUrl nvarchar(max) NULL;
END;
IF OBJECT_ID('dbo.SiteAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteAuditLogs (
        Id uniqueidentifier NOT NULL,
        EntityId uniqueidentifier NOT NULL,
        EntityType nvarchar(128) NOT NULL,
        Action nvarchar(max) NOT NULL,
        OldValue nvarchar(max) NULL,
        NewValue nvarchar(max) NULL,
        UserId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT PK_SiteAuditLogs PRIMARY KEY (Id),
        CONSTRAINT FK_SiteAuditLogs_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
IF OBJECT_ID('dbo.SiteAuditLogs', 'U') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM sys.columns
       WHERE object_id = OBJECT_ID('dbo.SiteAuditLogs')
         AND name = 'EntityType'
         AND max_length = -1
   )
BEGIN
    UPDATE dbo.SiteAuditLogs SET EntityType = LEFT(EntityType, 128) WHERE LEN(EntityType) > 128;
    ALTER TABLE dbo.SiteAuditLogs ALTER COLUMN EntityType nvarchar(128) NOT NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteAuditLogs_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.SiteAuditLogs'))
BEGIN
    CREATE INDEX IX_SiteAuditLogs_EntityType_EntityId ON dbo.SiteAuditLogs(EntityType, EntityId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteAuditLogs_UserId' AND object_id = OBJECT_ID('dbo.SiteAuditLogs'))
BEGIN
    CREATE INDEX IX_SiteAuditLogs_UserId ON dbo.SiteAuditLogs(UserId);
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
IF OBJECT_ID('dbo.StarredItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StarredItems (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        WorkspaceId uniqueidentifier NOT NULL,
        ItemType nvarchar(64) NOT NULL,
        ItemId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT PK_StarredItems PRIMARY KEY (Id),
        CONSTRAINT FK_StarredItems_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_StarredItems_Workspaces_WorkspaceId FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces(Id),
        CONSTRAINT CK_StarredItems_ItemType CHECK (ItemType IN ('Goal', 'Project', 'Team', 'User'))
    );
END;
IF OBJECT_ID('dbo.StarredItems', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.StarredItems', 'ItemType') = -1
BEGIN
    ALTER TABLE dbo.StarredItems ALTER COLUMN ItemType nvarchar(64) NOT NULL;
END;
IF OBJECT_ID('dbo.StarredItems', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_StarredItems_ItemType' AND parent_object_id = OBJECT_ID('dbo.StarredItems'))
BEGIN
    ALTER TABLE dbo.StarredItems ADD CONSTRAINT CK_StarredItems_ItemType CHECK (ItemType IN ('Goal', 'Project', 'Team', 'User'));
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StarredItems_UserId_WorkspaceId_ItemType_ItemId' AND object_id = OBJECT_ID('dbo.StarredItems'))
BEGIN
    CREATE UNIQUE INDEX IX_StarredItems_UserId_WorkspaceId_ItemType_ItemId ON dbo.StarredItems(UserId, WorkspaceId, ItemType, ItemId);
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comments (
        Id uniqueidentifier NOT NULL,
        EntityId uniqueidentifier NOT NULL,
        EntityType nvarchar(128) NOT NULL,
        UserId uniqueidentifier NOT NULL,
        Content nvarchar(max) NOT NULL,
        ParentCommentId uniqueidentifier NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_Comments_IsDeleted DEFAULT CAST(0 AS bit),
        CONSTRAINT PK_Comments PRIMARY KEY (Id),
        CONSTRAINT FK_Comments_Comments_ParentCommentId FOREIGN KEY (ParentCommentId) REFERENCES dbo.Comments(Id),
        CONSTRAINT FK_Comments_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Comments', 'EntityId') IS NULL
BEGIN
    ALTER TABLE dbo.Comments ADD EntityId uniqueidentifier NOT NULL CONSTRAINT DF_Comments_EntityId DEFAULT '00000000-0000-0000-0000-000000000000';
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Comments', 'EntityType') IS NULL
BEGIN
    ALTER TABLE dbo.Comments ADD EntityType nvarchar(128) NOT NULL CONSTRAINT DF_Comments_EntityType DEFAULT N'WorkTask';
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Comments', 'ParentCommentId') IS NULL
BEGIN
    ALTER TABLE dbo.Comments ADD ParentCommentId uniqueidentifier NULL;
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Comments', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE dbo.Comments ADD IsDeleted bit NOT NULL CONSTRAINT DF_Comments_IsDeleted DEFAULT CAST(0 AS bit);
END;
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM sys.columns
       WHERE object_id = OBJECT_ID('dbo.Comments')
         AND name = 'EntityType'
         AND max_length = -1
   )
BEGIN
    EXEC('UPDATE dbo.Comments SET EntityType = LEFT(EntityType, 128) WHERE LEN(EntityType) > 128;');
    EXEC('ALTER TABLE dbo.Comments ALTER COLUMN EntityType nvarchar(128) NOT NULL;');
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Comments_EntityType_EntityId' AND object_id = OBJECT_ID('dbo.Comments'))
BEGIN
    CREATE INDEX IX_Comments_EntityType_EntityId ON dbo.Comments(EntityType, EntityId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Comments_UserId' AND object_id = OBJECT_ID('dbo.Comments'))
BEGIN
    CREATE INDEX IX_Comments_UserId ON dbo.Comments(UserId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Comments_ParentCommentId' AND object_id = OBJECT_ID('dbo.Comments'))
BEGIN
    CREATE INDEX IX_Comments_ParentCommentId ON dbo.Comments(ParentCommentId);
END;
IF OBJECT_ID('dbo.CommentAttachments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommentAttachments (
        Id uniqueidentifier NOT NULL,
        CommentId uniqueidentifier NOT NULL,
        UploadedByUserId uniqueidentifier NOT NULL,
        FileName nvarchar(max) NOT NULL,
        FileUrl nvarchar(max) NOT NULL,
        ContentType nvarchar(max) NOT NULL,
        FileSize bigint NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT PK_CommentAttachments PRIMARY KEY (Id),
        CONSTRAINT FK_CommentAttachments_Comments_CommentId FOREIGN KEY (CommentId) REFERENCES dbo.Comments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CommentAttachments_Users_UploadedByUserId FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CommentAttachments_CommentId' AND object_id = OBJECT_ID('dbo.CommentAttachments'))
BEGIN
    CREATE INDEX IX_CommentAttachments_CommentId ON dbo.CommentAttachments(CommentId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CommentAttachments_UploadedByUserId' AND object_id = OBJECT_ID('dbo.CommentAttachments'))
BEGIN
    CREATE INDEX IX_CommentAttachments_UploadedByUserId ON dbo.CommentAttachments(UploadedByUserId);
END;
IF OBJECT_ID('dbo.CommentMentions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommentMentions (
        Id uniqueidentifier NOT NULL,
        CommentId uniqueidentifier NOT NULL,
        MentionedUserId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT PK_CommentMentions PRIMARY KEY (Id),
        CONSTRAINT FK_CommentMentions_Comments_CommentId FOREIGN KEY (CommentId) REFERENCES dbo.Comments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CommentMentions_Users_MentionedUserId FOREIGN KEY (MentionedUserId) REFERENCES dbo.Users(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CommentMentions_CommentId' AND object_id = OBJECT_ID('dbo.CommentMentions'))
BEGIN
    CREATE INDEX IX_CommentMentions_CommentId ON dbo.CommentMentions(CommentId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CommentMentions_MentionedUserId' AND object_id = OBJECT_ID('dbo.CommentMentions'))
BEGIN
    CREATE INDEX IX_CommentMentions_MentionedUserId ON dbo.CommentMentions(MentionedUserId);
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
IF OBJECT_ID('dbo.IntegrationAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntegrationAccounts (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        Provider nvarchar(128) NOT NULL,
        AccountEmail nvarchar(512) NOT NULL,
        ExternalAccountId nvarchar(512) NULL,
        AccessToken nvarchar(max) NOT NULL,
        RefreshToken nvarchar(max) NULL,
        AccessTokenExpiresAt datetime2 NULL,
        Scopes nvarchar(max) NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_IntegrationAccounts_IsActive DEFAULT CAST(1 AS bit),
        LastSyncedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_IntegrationAccounts_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_IntegrationAccounts_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_IntegrationAccounts PRIMARY KEY (Id),
        CONSTRAINT FK_IntegrationAccounts_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IntegrationAccounts_UserId_Provider' AND object_id = OBJECT_ID('dbo.IntegrationAccounts'))
BEGIN
    CREATE UNIQUE INDEX IX_IntegrationAccounts_UserId_Provider ON dbo.IntegrationAccounts(UserId, Provider);
END;
IF OBJECT_ID('dbo.InboxItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InboxItems (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        IntegrationAccountId uniqueidentifier NULL,
        Source nvarchar(64) NOT NULL,
        Provider nvarchar(128) NOT NULL,
        ExternalId nvarchar(512) NOT NULL,
        Title nvarchar(512) NOT NULL,
        Content nvarchar(max) NULL,
        Location nvarchar(512) NULL,
        StartsAt datetime2 NULL,
        EndsAt datetime2 NULL,
        IsRead bit NOT NULL CONSTRAINT DF_InboxItems_IsRead DEFAULT CAST(0 AS bit),
        CreatedTaskId uniqueidentifier NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_InboxItems_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_InboxItems_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_InboxItems PRIMARY KEY (Id),
        CONSTRAINT FK_InboxItems_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_InboxItems_IntegrationAccounts_IntegrationAccountId FOREIGN KEY (IntegrationAccountId) REFERENCES dbo.IntegrationAccounts(Id),
        CONSTRAINT FK_InboxItems_WorkTasks_CreatedTaskId FOREIGN KEY (CreatedTaskId) REFERENCES dbo.WorkTasks(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InboxItems_UserId_Provider_ExternalId' AND object_id = OBJECT_ID('dbo.InboxItems'))
BEGIN
    CREATE UNIQUE INDEX IX_InboxItems_UserId_Provider_ExternalId ON dbo.InboxItems(UserId, Provider, ExternalId);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InboxItems_UserId_Source_CreatedAt' AND object_id = OBJECT_ID('dbo.InboxItems'))
BEGIN
    CREATE INDEX IX_InboxItems_UserId_Source_CreatedAt ON dbo.InboxItems(UserId, Source, CreatedAt);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InboxItems_UserId_IsRead' AND object_id = OBJECT_ID('dbo.InboxItems'))
BEGIN
    CREATE INDEX IX_InboxItems_UserId_IsRead ON dbo.InboxItems(UserId, IsRead);
END;
IF OBJECT_ID('dbo.SyncHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncHistories (
        Id uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        IntegrationAccountId uniqueidentifier NULL,
        Provider nvarchar(128) NOT NULL,
        Status nvarchar(64) NOT NULL,
        ItemsImported int NOT NULL CONSTRAINT DF_SyncHistories_ItemsImported DEFAULT 0,
        Message nvarchar(max) NULL,
        StartedAt datetime2 NOT NULL CONSTRAINT DF_SyncHistories_StartedAt DEFAULT SYSUTCDATETIME(),
        CompletedAt datetime2 NULL,
        CONSTRAINT PK_SyncHistories PRIMARY KEY (Id),
        CONSTRAINT FK_SyncHistories_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_SyncHistories_IntegrationAccounts_IntegrationAccountId FOREIGN KEY (IntegrationAccountId) REFERENCES dbo.IntegrationAccounts(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SyncHistories_UserId_Provider_StartedAt' AND object_id = OBJECT_ID('dbo.SyncHistories'))
BEGIN
    CREATE INDEX IX_SyncHistories_UserId_Provider_StartedAt ON dbo.SyncHistories(UserId, Provider, StartedAt);
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
