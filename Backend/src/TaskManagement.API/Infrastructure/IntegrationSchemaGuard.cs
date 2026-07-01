using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Infrastructure
{
    public static class IntegrationSchemaGuard
    {
        public static async Task EnsureCreatedAsync(ApplicationDbContext context)
        {
            if (!context.Database.IsRelational())
            {
                return;
            }

            await context.Database.ExecuteSqlRawAsync(@"
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
");
        }
    }
}
