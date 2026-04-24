using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

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

        public async Task BroadcastProjectEvent(string projectId, string eventType, string? payloadJson)
        {
            object? payload = null;

            if (!string.IsNullOrWhiteSpace(payloadJson))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<object>(payloadJson);
                }
                catch
                {
                    payload = payloadJson;
                }
            }

            await Clients.OthersInGroup(projectId).SendAsync("ProjectRealtimeEvent", new
            {
                projectId,
                type = eventType,
                payload
            });
        }
    }
}
