using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.API.Hubs
{
    public class KanbanHub : Hub
    {
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // SignalR automatically removes ConnectionId from all groups,
            // but we override it here for explicit "dọn rác" as per BusinessLogic2.md requirements.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
