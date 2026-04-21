using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services
{
    public class GamificationService : IGamificationService
    {
        private const string RewardType = "TaskDoneReward";
        private const string RollbackType = "TaskDoneRollback";
        private const string AssignmentRewardType = "AssignmentDoneReward";
        private const string EarlyBonusType = "AssignmentEarlyBonus";
        private readonly ApplicationDbContext _context;

        public GamificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ApplyStatusChangeRewardsAsync(Guid workTaskId, Guid actorUserId, string? oldStatusName, string? newStatusName)
        {
            var wasDone = IsDone(oldStatusName);
            var isDone = IsDone(newStatusName);

            if (wasDone == isDone)
            {
                return;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var task = await _context.WorkTasks
                        .AsNoTracking()
                        .Include(t => t.TaskAssignments)
                        .FirstOrDefaultAsync(t => t.Id == workTaskId && !t.IsDeleted);

                    if (task == null)
                    {
                        throw new ArgumentException("Task khong ton tai.");
                    }

                    if (task.TaskAssignments.Any(ta => ta.Status))
                    {
                        await transaction.CommitAsync();
                        return;
                    }

                    var targetUserId = task.AssignedUserId ?? task.ReporterId;
    
                    var wallet = await _context.UserWallets
                        .FirstOrDefaultAsync(w => w.UserId == targetUserId);
    
                    if (wallet == null)
                    {
                        wallet = new UserWallet
                        {
                            UserId = targetUserId,
                            TotalPoints = 0,
                            Level = 1
                        };
                        _context.UserWallets.Add(wallet);
                        await _context.SaveChangesAsync();
                    }
    
                    if (!wasDone && isDone)
                    {
                        var alreadyRewarded = await _context.PointTransactions.AnyAsync(pt =>
                            pt.UserWalletUserId == targetUserId &&
                            pt.WorkTaskId == workTaskId &&
                            pt.TransactionType == RewardType);
    
                        if (!alreadyRewarded)
                        {
                            var points = CalculateRewardPoints(task);
                            wallet.TotalPoints += points;
                            wallet.Level = CalculateLevel(wallet.TotalPoints);

                            _context.PointTransactions.Add(new PointTransaction
                            {
                                Id = Guid.NewGuid(),
                                UserWalletUserId = targetUserId,
                                WorkTaskId = workTaskId,
                                Amount = points,
                                TransactionType = RewardType,
                                Reason = $"Cong diem hoan thanh task {task.SequenceId ?? task.Id.ToString()}",
                                CreatedAt = DateTime.UtcNow
                            });

                            if (task.DueDate.HasValue && DateTime.UtcNow <= task.DueDate.Value)
                            {
                                var bonus = Math.Max(1, (int)Math.Round(points * 0.1, MidpointRounding.AwayFromZero));
                                wallet.TotalPoints += bonus;
                                wallet.Level = CalculateLevel(wallet.TotalPoints);

                                _context.PointTransactions.Add(new PointTransaction
                                {
                                    Id = Guid.NewGuid(),
                                    UserWalletUserId = targetUserId,
                                    WorkTaskId = workTaskId,
                                    Amount = bonus,
                                    TransactionType = EarlyBonusType,
                                    Reason = $"Thuong hoan thanh som task {task.SequenceId ?? task.Id.ToString()}",
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                    else if (wasDone && !isDone)
                    {
                        var rewardPoints = await _context.PointTransactions
                            .Where(pt =>
                                pt.UserWalletUserId == targetUserId &&
                                pt.WorkTaskId == workTaskId &&
                                pt.TransactionType == RewardType)
                            .SumAsync(pt => (int?)pt.Amount) ?? 0;

                        var bonusPoints = await _context.PointTransactions
                            .Where(pt =>
                                pt.UserWalletUserId == targetUserId &&
                                pt.WorkTaskId == workTaskId &&
                                pt.TransactionType == EarlyBonusType)
                            .SumAsync(pt => (int?)pt.Amount) ?? 0;

                        var rollbackPoints = await _context.PointTransactions
                            .Where(pt =>
                                pt.UserWalletUserId == targetUserId &&
                                pt.WorkTaskId == workTaskId &&
                                pt.TransactionType == RollbackType)
                            .SumAsync(pt => (int?)Math.Abs(pt.Amount)) ?? 0;

                        var remainingRollback = Math.Max(0, rewardPoints + bonusPoints - rollbackPoints);
                        if (remainingRollback > 0)
                        {
                            wallet.TotalPoints = Math.Max(0, wallet.TotalPoints - remainingRollback);
                            wallet.Level = CalculateLevel(wallet.TotalPoints);
    
                            _context.PointTransactions.Add(new PointTransaction
                            {
                                Id = Guid.NewGuid(),
                                UserWalletUserId = targetUserId,
                                WorkTaskId = workTaskId,
                                Amount = -remainingRollback,
                                TransactionType = RollbackType,
                                Reason = $"Hoan tra diem do Task bi Reject boi user {actorUserId}",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task ApplyAssignmentProgressRewardsAsync(Guid workTaskId, Guid assigneeUserId, Guid actorUserId, double oldProgressPercent, double newProgressPercent)
        {
            if (oldProgressPercent >= 100 || newProgressPercent < 100)
            {
                return;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var task = await _context.WorkTasks
                        .AsNoTracking()
                        .Include(t => t.TaskAssignments)
                        .FirstOrDefaultAsync(t => t.Id == workTaskId && !t.IsDeleted);

                    if (task == null)
                    {
                        throw new ArgumentException("Task khong ton tai.");
                    }

                    var assignment = task.TaskAssignments.FirstOrDefault(ta => ta.UserId == assigneeUserId && ta.Status);
                    if (assignment == null)
                    {
                        return;
                    }

                    var alreadyRewarded = await _context.PointTransactions.AnyAsync(pt =>
                        pt.UserWalletUserId == assigneeUserId &&
                        pt.WorkTaskId == workTaskId &&
                        pt.TransactionType == AssignmentRewardType);

                    if (alreadyRewarded)
                    {
                        return;
                    }

                    var wallet = await GetOrCreateWalletAsync(assigneeUserId);
                    var basePoints = CalculateRewardPoints(task);
                    var activeAssignments = task.TaskAssignments.Where(ta => ta.Status).ToList();
                    var totalWeight = activeAssignments.Sum(ta => Math.Max(ta.ContributionWeight, 0));
                    var userWeight = Math.Max(assignment.ContributionWeight, 0);
                    var share = totalWeight > 0
                        ? userWeight / totalWeight
                        : 1.0 / Math.Max(activeAssignments.Count, 1);
                    var points = Math.Max(1, (int)Math.Round(basePoints * share, MidpointRounding.AwayFromZero));

                    wallet.TotalPoints += points;
                    wallet.Level = CalculateLevel(wallet.TotalPoints);
                    _context.PointTransactions.Add(new PointTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserWalletUserId = assigneeUserId,
                        WorkTaskId = workTaskId,
                        Amount = points,
                        TransactionType = AssignmentRewardType,
                        Reason = $"Cong diem hoan thanh phan viec task {task.SequenceId ?? task.Id.ToString()}",
                        CreatedAt = DateTime.UtcNow
                    });

                    if (task.DueDate.HasValue)
                    {
                        var now = DateTime.UtcNow;
                        if (now <= task.DueDate.Value)
                        {
                            var bonus = Math.Max(1, (int)Math.Round(points * 0.1, MidpointRounding.AwayFromZero));
                            wallet.TotalPoints += bonus;
                            _context.PointTransactions.Add(new PointTransaction
                            {
                                Id = Guid.NewGuid(),
                                UserWalletUserId = assigneeUserId,
                                WorkTaskId = workTaskId,
                                Amount = bonus,
                                TransactionType = EarlyBonusType,
                                Reason = $"Thuong hoan thanh som phan viec task {task.SequenceId ?? task.Id.ToString()}",
                                CreatedAt = now
                            });
                        }

                        wallet.Level = CalculateLevel(wallet.TotalPoints);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task<UserWallet> GetOrCreateWalletAsync(Guid userId)
        {
            var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new UserWallet
            {
                UserId = userId,
                TotalPoints = 0,
                Level = 1
            };
            _context.UserWallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        private static bool IsDone(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return false;
            }

            var normalized = NormalizeStatus(statusName);
            return normalized.Contains("DONE") ||
                   normalized.Contains("COMPLETE") ||
                   normalized.Contains("HOAN THANH") ||
                   normalized.Contains("HOÀN THÀNH");
        }

        private static string NormalizeStatus(string statusName)
        {
            var decomposed = statusName.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        private static int CalculateRewardPoints(TaskManagement.Domain.Entities.WorkTask task)
        {
            var value = Math.Max(1, (int)Math.Round(task.StoryPoints <= 0 ? 1 : task.StoryPoints, MidpointRounding.AwayFromZero));
            var impact = task.Priority switch
            {
                1 => 4,
                2 => 3,
                3 => 2,
                _ => 1
            };

            var endDate = task.PlannedEndDate ?? task.DueDate;
            var days = 1;
            if (task.PlannedStartDate.HasValue && endDate.HasValue)
            {
                days = Math.Max(1, (endDate.Value.Date - task.PlannedStartDate.Value.Date).Days + 1);
            }

            return value * impact * days;
        }

        private static int CalculateLevel(int totalPoints)
        {
            var level = 1;
            var nextThreshold = PointsForCareerLevel(level + 1);

            while (totalPoints >= nextThreshold)
            {
                level++;
                nextThreshold = PointsForCareerLevel(level + 1);
            }

            return level;
        }

        private static int PointsForCareerLevel(int level)
        {
            var normalizedLevel = Math.Max(1, level);
            return 250 * normalizedLevel * (normalizedLevel + 1);
        }
    }
}
