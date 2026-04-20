using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace notification_service.Hubs;

[Authorize]
public class NotificationsHub : Hub<INotificationsClient>
{
    public static string GetGroupName(Guid userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }
}