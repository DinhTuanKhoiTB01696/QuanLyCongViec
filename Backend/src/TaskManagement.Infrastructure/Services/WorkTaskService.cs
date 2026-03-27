using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.WorkTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly ApplicationDbContext _context;

        public WorkTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequestDto request)
        {
            var taskToUpdate = await _context.WorkTasks
                .Include(wt => wt.TaskStatus)
                .FirstOrDefaultAsync(wt => wt.Id == taskId && !wt.IsDeleted);

            if (taskToUpdate == null) throw new ArgumentException("Tác vụ không tồn tại.");

            var oldStatus = taskToUpdate.TaskStatus;
            var newStatus = await _context.TaskStatuses.FirstOrDefaultAsync(ts => ts.Id == request.TaskStatusId);

            if (newStatus == null) throw new ArgumentException("Trạng thái chuyển đến không tồn tại.");

            if (oldStatus.Id == newStatus.Id) return;

            // 1. State Machine Guardrails: Prevent skipping statuses
            if (Math.Abs(oldStatus.Position - newStatus.Position) > 1)
            {
                throw new InvalidOperationException("Không được phép nhảy cóc trạng thái. Vui lòng chuyển thẻ theo thứ tự.");
            }

            bool isMovingToActiveOrDone = newStatus.Name.Contains("In Progress", StringComparison.OrdinalIgnoreCase) || 
                                          newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Đang làm", StringComparison.OrdinalIgnoreCase) ||
                                          newStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);

            // 2. Task Dependencies / Blockers
            if (isMovingToActiveOrDone)
            {
                var blockers = await _context.TaskDependencies
                    .Include(td => td.PredecessorTask)
                    .ThenInclude(pt => pt.TaskStatus)
                    .Where(td => td.SuccessorTaskId == taskId)
                    .Select(td => td.PredecessorTask)
                    .ToListAsync();

                foreach (var blocker in blockers)
                {
                    bool isBlockerDone = blocker.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) ||
                                         blocker.TaskStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);
                    if (!isBlockerDone)
                    {
                        throw new InvalidOperationException($"Tác vụ phụ thuộc '{blocker.Title}' chưa hoàn thành. Không thể chuyển trạng thái.");
                    }
                }
            }

            // 3. Parent-Subtask Constraint
            bool isMovingToDone = newStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) || 
                                  newStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase);

            if (isMovingToDone)
            {
                bool hasUnfinishedSubtasks = await _context.WorkTasks
                    .Include(wt => wt.TaskStatus)
                    .AnyAsync(wt => wt.ParentTaskId == taskId && !wt.IsDeleted && 
                                    !wt.TaskStatus.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) &&
                                    !wt.TaskStatus.Name.Contains("Hoàn thành", StringComparison.OrdinalIgnoreCase));

                if (hasUnfinishedSubtasks)
                {
                    throw new InvalidOperationException("Không thể hoàn thành tác vụ cha khi vẫn còn tác vụ con (subtask) chưa hoàn thành.");
                }
            }

            // Apply modifications
            taskToUpdate.TaskStatusId = newStatus.Id;
            taskToUpdate.UpdatedAt = DateTime.UtcNow; // UTC enforcement for UpdatedAt
            
            // Apply RowVersion provided from client for Concurrency Check
            if (request.RowVersion != null && request.RowVersion.Length > 0)
            {
                _context.Entry(taskToUpdate).Property(nameof(taskToUpdate.RowVersion)).OriginalValue = request.RowVersion;
            }

            await _context.SaveChangesAsync();
        }
    }
}
