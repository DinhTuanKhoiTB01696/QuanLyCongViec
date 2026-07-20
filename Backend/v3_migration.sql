BEGIN TRANSACTION;
ALTER TABLE [ContingencyPlans] DROP CONSTRAINT [FK_ContingencyPlans_Users_ActivatedById];

ALTER TABLE [ContingencyPlans] DROP CONSTRAINT [FK_ContingencyPlans_WorkTasks_ContingencyTaskId];

DROP INDEX [IX_ContingencyPlans_ActivatedById] ON [ContingencyPlans];

DROP INDEX [IX_ContingencyPlans_ContingencyTaskId] ON [ContingencyPlans];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'ActivatedAt');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [ActivatedAt];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'ActivatedById');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [ActivatedById];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'ActivationCondition');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [ActivationCondition];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'ContingencyTaskId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [ContingencyTaskId];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'IsActivated');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [IsActivated];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContingencyPlans]') AND [c].[name] = N'RiskStatus');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ContingencyPlans] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [ContingencyPlans] DROP COLUMN [RiskStatus];

CREATE TABLE [ContingencyPlanTasks] (
    [Id] uniqueidentifier NOT NULL,
    [ContingencyPlanId] uniqueidentifier NOT NULL,
    [WorkTaskId] uniqueidentifier NOT NULL,
    [IsActivated] bit NOT NULL,
    [ActivatedById] uniqueidentifier NULL,
    [ActivatedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ContingencyPlanTasks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContingencyPlanTasks_ContingencyPlans_ContingencyPlanId] FOREIGN KEY ([ContingencyPlanId]) REFERENCES [ContingencyPlans] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContingencyPlanTasks_Users_ActivatedById] FOREIGN KEY ([ActivatedById]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ContingencyPlanTasks_WorkTasks_WorkTaskId] FOREIGN KEY ([WorkTaskId]) REFERENCES [WorkTasks] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ContingencyPlanTasks_ActivatedById] ON [ContingencyPlanTasks] ([ActivatedById]);

CREATE INDEX [IX_ContingencyPlanTasks_ContingencyPlanId] ON [ContingencyPlanTasks] ([ContingencyPlanId]);

CREATE INDEX [IX_ContingencyPlanTasks_WorkTaskId] ON [ContingencyPlanTasks] ([WorkTaskId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260714091551_UpdateContingencyPlanSchemaV3', N'10.0.5');

COMMIT;
GO

