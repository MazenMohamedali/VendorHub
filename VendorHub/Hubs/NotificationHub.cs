using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using VendorHub.Services;

namespace VendorHub.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly NotificationService _notificationService;
        public NotificationHub(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        #region lifeCycle events
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            if (userRole == "Vendor")
                await Groups.AddToGroupAsync(Context.ConnectionId, $"vendor-{userId}");

            else if (userRole == "Admin")
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

            else if (userRole == "Customer")
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region ClientMethod
        public async Task SendMessage(string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            await Clients.All.SendAsync("ReceiveMessage", new
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Not authenticated");
                return;
            }

            var result = await _notificationService.MarkAsReadAsync(notificationId);

            if (result.Success)
            {
                await Clients.Caller.SendAsync("NotificationMarkedRead", notificationId);
                return;
            }

            await Clients.Caller.SendAsync("Error", result.Message);
        }
        #endregion
    }
}
