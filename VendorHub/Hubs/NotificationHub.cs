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
        public async Task JoinVendorGroup(int vendorId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != vendorId.ToString())
            {
                await Clients.Caller.SendAsync("Error", "You can only join your own vendor group");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vendor-{vendorId}");
            await Clients.Caller.SendAsync("GroupJoined", $"vendor-{vendorId}");
            Console.WriteLine($"✅ Vendor {vendorId} explicitly joined group vendor-{vendorId}");
        }

        public async Task JoinCustomerGroup(int customerId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != customerId.ToString())
            {
                await Clients.Caller.SendAsync("Error", "You can only join your own customer group");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{customerId}");
            await Clients.Caller.SendAsync("GroupJoined", $"user-{customerId}");
            Console.WriteLine($"✅ Customer {customerId} explicitly joined group user-{customerId}");
        }

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
