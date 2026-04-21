IF COL_LENGTH('dbo.TaskDrafts', 'ProjectId') IS NULL
BEGIN
    ALTER TABLE dbo.TaskDrafts ADD ProjectId uniqueidentifier NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskDrafts_UserId_ProjectId_UpdatedAt' AND object_id = OBJECT_ID('dbo.TaskDrafts'))
BEGIN
    CREATE INDEX IX_TaskDrafts_UserId_ProjectId_UpdatedAt ON dbo.TaskDrafts(UserId, ProjectId, UpdatedAt);
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

UPDATE td
SET ProjectId = TRY_CONVERT(uniqueidentifier, JSON_VALUE(td.PayloadJson, '$.projectId'))
FROM dbo.TaskDrafts td
WHERE td.ProjectId IS NULL
  AND ISJSON(td.PayloadJson) = 1
  AND JSON_VALUE(td.PayloadJson, '$.projectId') IS NOT NULL;
