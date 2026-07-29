using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            int userId = this.GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{userId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vendor-{userId}");

            await base.OnConnectedAsync();
        }   

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int userId = this.GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{userId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vendor-{userId}");
            await base.OnDisconnectedAsync(exception);
        }

    }
}
