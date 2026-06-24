using System;

namespace TaskManagement.Application.Common.Security
{
    public static class HomeSitePermissions
    {
        // Simple permission rules for Home Site operations.
        // In a real enterprise system, these might be evaluated against a database of policies,
        // but since we want standard Jira-like defaults, we use these static helpers.

        public static bool CanEditComment(Guid userId, Guid commentAuthorId, string userRole)
        {
            // The author can edit their own comment. Admins can edit any comment.
            return userId == commentAuthorId || userRole == "Admin" || userRole == "Owner";
        }

        public static bool CanDeleteComment(Guid userId, Guid commentAuthorId, string userRole)
        {
            // The author or Admin can delete a comment.
            return userId == commentAuthorId || userRole == "Admin" || userRole == "Owner";
        }

        public static bool CanChangeTeamManager(string userRole)
        {
            // Only Workspace Admins/Owners can change team managers in the Home Site.
            return userRole == "Admin" || userRole == "Owner";
        }

        public static bool CanArchiveGoalOrProject(Guid userId, Guid ownerId, string userRole)
        {
            // The creator/owner or Admin can archive.
            return userId == ownerId || userRole == "Admin" || userRole == "Owner";
        }

        public static bool CanUpdateGoalProgress(Guid userId, Guid ownerId, string userRole)
        {
            // The owner of the goal, or Workspace Admins.
            // Teams assigned to the goal might also be allowed (requires DB check), 
            // but for simplicity here we check the primary owner.
            return userId == ownerId || userRole == "Admin" || userRole == "Owner";
        }
    }
}
