const fs = require('fs');
const path = require('path');

function generateGuid(index, prefix) {
    // A simple deterministic GUID generator based on index and prefix
    const hex = index.toString(16).padStart(4, '0');
    return `${prefix}-0000-0000-0000-00000000${hex}`;
}

const roles = [
    'PM', 'PO', 'DEV', 'SM', 'PA', 'Accountant', 'Marketing', 'System Admin', 
    'QA/Tester', 'UI/UX Designer', 'Stakeholder/Client', 'DevOps / Deployment',
    'HR Manager', 'Sales Representative', 'Customer Support', 'Business Analyst',
    'Content Writer', 'Legal Advisor', 'Data Scientist', 'Security Engineer'
];

let sql = `-- =======================================================
-- SEED DATA SCRIPT FOR TASK MANAGEMENT DATABASE
-- Generates at least 20 rows per table with explicit FKs
-- =======================================================\n\n`;

const USERS_PREFIX = '11111111';
const ROLES_PREFIX = '22222222';
const PERM_PREFIX = '33333333';
const DEPT_PREFIX = '44444444';
const PROJ_PREFIX = '55555555';
const SPRINT_PREFIX = '66666666';
const TYPE_PREFIX = '77777777';
const STAT_PREFIX = '88888888';
const TASK_PREFIX = '99999999';
const COMM_PREFIX = 'aaaaaaaa';
const ATTA_PREFIX = 'bbbbbbbb';
const AUDIT_PREFIX = 'cccccccc';
const NOTIF_PREFIX = 'dddddddd';
const REVIEW_PREFIX = 'eeeeeeee';
const WALLET_PREFIX = 'ffffffff'; // Actually user id, so we use user ids
const TRAN_PREFIX = '10101010';
const PROMPT_PREFIX = '20202020';
const USAGE_PREFIX = '30303030';
const FEED_PREFIX = '40404040';
const DATASET_PREFIX = '50505050';
const TIME_PREFIX = '70707070';
const ROW_COUNT = 20;

// 1. Users
sql += `-- Users\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO Users (Id, Email, PasswordHash, FullName, IsActive, IsDeleted, CreatedAt, UpdatedAt) VALUES ('${id}', 'user${i}@test.com', 'hash${i}', 'User ${i}', 1, 0, GETDATE(), GETDATE());\n`;
}
sql += '\n';

// 2. Roles
sql += `-- Roles\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, ROLES_PREFIX);
    sql += `INSERT INTO Roles (Id, Name, Description) VALUES ('${id}', '${roles[i-1]}', 'Description for ${roles[i-1]}');\n`;
}
sql += '\n';

// 3. UserRoles
sql += `-- UserRoles\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let uid = generateGuid(i, USERS_PREFIX);
    let rid = generateGuid(i, ROLES_PREFIX);
    sql += `INSERT INTO UserRoles (UserId, RoleId) VALUES ('${uid}', '${rid}');\n`;
}
sql += '\n';

// 4. Permissions
sql += `-- Permissions\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, PERM_PREFIX);
    sql += `INSERT INTO Permissions (Id, Code, Module) VALUES ('${id}', 'PERM_${i}', 'Module${(i%5)+1}');\n`;
}
sql += '\n';

// 5. RolePermissions
sql += `-- RolePermissions\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let rid = generateGuid(i, ROLES_PREFIX);
    let pid = generateGuid(i, PERM_PREFIX);
    sql += `INSERT INTO RolePermissions (RoleId, PermissionId) VALUES ('${rid}', '${pid}');\n`;
}
sql += '\n';

// 6. Departments
sql += `-- Departments\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, DEPT_PREFIX);
    let managerId = generateGuid(i, USERS_PREFIX); // Use user i as manager
    sql += `INSERT INTO Departments (Id, Name, ManagerId, CreatedAt, IsActive, IsDeleted) VALUES ('${id}', 'Department ${i}', '${managerId}', GETDATE(), 1, 0);\n`;
}
sql += '\n';

// 7. DepartmentMembers
sql += `-- DepartmentMembers\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let did = generateGuid(i, DEPT_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO DepartmentMembers (DepartmentId, UserId, JoinedAt) VALUES ('${did}', '${uid}', GETDATE());\n`;
}
sql += '\n';

// 8. Projects
sql += `-- Projects\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, PROJ_PREFIX);
    let creatorId = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO Projects (Id, Name, Description, StartDate, EndDate, Status, IsDeleted, CreatorId, CreatedAt, UpdatedAt) VALUES ('${id}', 'Project ${i}', 'Proj Desc ${i}', GETDATE(), DATEADD(month, 3, GETDATE()), ${(i%3)}, 0, '${creatorId}', GETDATE(), GETDATE());\n`;
}
sql += '\n';

// 9. ProjectMembers
sql += `-- ProjectMembers\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let pid = generateGuid(i, PROJ_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO ProjectMembers (ProjectId, UserId, ProjectRole, JoinedAt, Status) VALUES ('${pid}', '${uid}', 'DEV', GETDATE(), 1);\n`;
}
sql += '\n';

// 10. Sprints
sql += `-- Sprints\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, SPRINT_PREFIX);
    let pid = generateGuid(i, PROJ_PREFIX); // link sprint i to project i
    sql += `INSERT INTO Sprints (Id, Name, StartDate, EndDate, ProjectId, Status, CreatedAt) VALUES ('${id}', 'Sprint ${i}', GETDATE(), DATEADD(day, 14, GETDATE()), '${pid}', ${(i%2)}, GETDATE());\n`;
}
sql += '\n';

// 11. TaskTypes
sql += `-- TaskTypes\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, TYPE_PREFIX);
    let pid = generateGuid(i, PROJ_PREFIX);
    sql += `INSERT INTO TaskTypes (Id, Name, ColorCode, Icon, ProjectId) VALUES ('${id}', 'Type ${i}', '#FFFFFF', 'icon-${i}', '${pid}');\n`;
}
sql += '\n';

// 12. TaskStatuses
sql += `-- TaskStatuses\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, STAT_PREFIX);
    let pid = generateGuid(i, PROJ_PREFIX);
    sql += `INSERT INTO TaskStatuses (Id, Name, ColorCode, Position, ProjectId) VALUES ('${id}', 'Status ${i}', '#000000', ${i}, '${pid}');\n`;
}
sql += '\n';

// 13. WorkTasks
sql += `-- WorkTasks\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, TASK_PREFIX);
    let pid = generateGuid((i%20)+1, PROJ_PREFIX);
    let sid = generateGuid((i%20)+1, SPRINT_PREFIX);
    let tid = generateGuid((i%20)+1, TYPE_PREFIX);
    let tsid = generateGuid((i%20)+1, STAT_PREFIX);
    let rid = generateGuid((i%20)+1, USERS_PREFIX);
    sql += `INSERT INTO WorkTasks (Id, Title, Description, Priority, StoryPoints, PlannedStartDate, PlannedEndDate, IsDeleted, ProjectId, SprintId, TaskTypeId, TaskStatusId, ReporterId, TotalEstimatedHours, TotalActualHours, CreatedAt, UpdatedAt) VALUES ('${id}', 'Task ${i}', 'Desc ${i}', ${(i%4)}, ${i%8}, GETDATE(), DATEADD(day, 5, GETDATE()), 0, '${pid}', '${sid}', '${tid}', '${tsid}', '${rid}', 8, 4, GETDATE(), GETDATE());\n`;
}
sql += '\n';

// 14. TaskAssignments
sql += `-- TaskAssignments\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let tid = generateGuid(i, TASK_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO TaskAssignments (WorkTaskId, UserId, Status, Priority, EstimatedHours, TotalActualHours, ActualStartDate, ActualEndDate) VALUES ('${tid}', '${uid}', 0, ${(i%4)}, 4.0, 0, GETDATE(), DATEADD(day, 2, GETDATE()));\n`;
}
sql += '\n';

// 15. TaskDependencies
sql += `-- TaskDependencies\n`;
for (let i = 1; i <= ROW_COUNT-1; i++) {
    // Make task i depend on task i+1
    let pid = generateGuid(i+1, TASK_PREFIX);
    let sid = generateGuid(i, TASK_PREFIX);
    sql += `INSERT INTO TaskDependencies (PredecessorTaskId, SuccessorTaskId, DependencyType) VALUES ('${pid}', '${sid}', 0);\n`;
}
// 20th row specifically
sql += `INSERT INTO TaskDependencies (PredecessorTaskId, SuccessorTaskId, DependencyType) VALUES ('${generateGuid(1, TASK_PREFIX)}', '${generateGuid(20, TASK_PREFIX)}', 0);\n`;
sql += '\n';

// 16. Comments
sql += `-- Comments\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, COMM_PREFIX);
    let tid = generateGuid(i, TASK_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO Comments (Id, Content, EntityId, EntityType, UserId, IsDeleted, CreatedAt, UpdatedAt) VALUES ('${id}', 'Comment ${i}', '${tid}', 'WorkTask', '${uid}', 0, GETDATE(), GETDATE());\n`;
}
sql += '\n';

// 17. Attachments
sql += `-- Attachments\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, ATTA_PREFIX);
    let tid = generateGuid(i, TASK_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO Attachments (Id, FileName, FileUrl, FileSize, WorkTaskId, UserId, CreatedAt) VALUES ('${id}', 'file${i}.pdf', '/files/file${i}.pdf', 1024, '${tid}', '${uid}', GETDATE());\n`;
}
sql += '\n';

// 18. AuditLogs
sql += `-- AuditLogs\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, AUDIT_PREFIX);
    let tid = generateGuid(i, TASK_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO AuditLogs (Id, WorkTaskId, UserId, FieldChanged, OldValue, NewValue, CreatedAt) VALUES ('${id}', '${tid}', '${uid}', 'Status', 'To Do', 'In Progress', GETDATE());\n`;
}
sql += '\n';

// 19. Notifications
sql += `-- Notifications\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, NOTIF_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO Notifications (Id, Title, Content, LinkUrl, IsRead, UserId, CreatedAt) VALUES ('${id}', 'Notif ${i}', 'Content ${i}', '/tasks/${id}', 0, '${uid}', GETDATE());\n`;
}
sql += '\n';

// 20. PerformanceReviews
sql += `-- PerformanceReviews\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, REVIEW_PREFIX);
    let reviewer = generateGuid(i, USERS_PREFIX);
    // reviewee is next user, or user 1 for the last one
    let reviewee = generateGuid(i === ROW_COUNT ? 1 : i + 1, USERS_PREFIX);
    sql += `INSERT INTO PerformanceReviews (Id, ReviewerId, RevieweeId, Score, Feedback, ReviewPeriod, CreatedAt) VALUES ('${id}', '${reviewer}', '${reviewee}', ${(i%10)+1}, 'Feedback ${i}', '2026-Q1', GETDATE());\n`;
}
sql += '\n';

// 21. UserWallets
sql += `-- UserWallets\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO UserWallets (UserId, TotalPoints, Level) VALUES ('${uid}', 1000, 1);\n`;
}
sql += '\n';

// 22. PointTransactions
sql += `-- PointTransactions\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, TRAN_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO PointTransactions (Id, UserWalletUserId, Amount, Reason, CreatedAt) VALUES ('${id}', '${uid}', 10, 'Completed Task ${i}', GETDATE());\n`;
}
sql += '\n';

// 23. AIPromptTemplates
sql += `-- AIPromptTemplates\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, PROMPT_PREFIX);
    sql += `INSERT INTO AIPromptTemplates (Id, Code, TemplateContent, IsActive) VALUES ('${id}', 'PROMPT_${i}', 'Template content ${i}', 1);\n`;
}
sql += '\n';

// 24. AITokenUsages
sql += `-- AITokenUsages\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, USAGE_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO AITokenUsages (Id, UserId, FeatureCode, TokensUsed, CreatedAt) VALUES ('${id}', '${uid}', 'FEATURE_${i}', 30, GETDATE());\n`;
}
sql += '\n';

// 25. AIFeedbacks
sql += `-- AIFeedbacks\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, FEED_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO AIFeedbacks (Id, UserId, PromptContent, AIResponse, Rating, CreatedAt) VALUES ('${id}', '${uid}', 'Prompt content ${i}', 'AI response ${i}', 5, GETDATE());\n`;
}
sql += '\n';

// 26. AITrainingDatasets
sql += `-- AITrainingDatasets\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, DATASET_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO AITrainingDatasets (Id, Category, InputData, OutputData, IsApproved, CreatorId, CreatedAt) VALUES ('${id}', 'Category ${i}', 'Input ${i}', 'Output ${i}', 1, '${uid}', GETDATE());\n`;
}
sql += '\n';

// 27. TaskVectorEmbeddings
sql += `-- TaskVectorEmbeddings\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let tid = generateGuid(i, TASK_PREFIX);
    sql += `INSERT INTO TaskVectorEmbeddings (WorkTaskId, VectorData, LastCalculatedAt) VALUES ('${tid}', '[]', GETDATE());\n`;
}
sql += '\n';

// 28. TimeLogs
sql += `-- TimeLogs\n`;
for (let i = 1; i <= ROW_COUNT; i++) {
    let id = generateGuid(i, TIME_PREFIX);
    let tid = generateGuid(i, TASK_PREFIX);
    let uid = generateGuid(i, USERS_PREFIX);
    sql += `INSERT INTO TimeLogs (Id, WorkTaskId, UserId, Hours, WorkType, Note, LoggedAt) VALUES ('${id}', '${tid}', '${uid}', 2, 'Dev', 'Note ${i}', GETDATE());\n`;
}
sql += '\n';

fs.writeFileSync(path.join(__dirname, 'seed_data.sql'), sql);
console.log('Successfully generated seed_data.sql');
