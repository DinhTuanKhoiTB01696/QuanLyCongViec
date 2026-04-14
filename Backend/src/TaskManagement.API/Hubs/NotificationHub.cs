using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notification push.
    /// Each user joins a personal group "user_{userId}" on connect.
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Client calls this after connecting to subscribe to their personal notification channel
        /// </summary>
        public async Task JoinUserChannel(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserChannel(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}
