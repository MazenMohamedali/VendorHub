using Microsoft.AspNetCore.SignalR;
using VendorHub.Hubs;
using VendorHub.Models;

namespace VendorHub.Events
{
    public class SignalrOrderStatusChangedHandler : ICustomEventHandler<OrderStatusChangedEvent>
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public SignalrOrderStatusChangedHandler(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleAsync(OrderStatusChangedEvent evnt)
        {
            var payload = new
            {
                OrderId = evnt.OrderId,
                Status = evnt.NewStatus,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"customer-{evnt.CustomerId}")
                .ReceiveOrderStatusNotification(payload);
        }
    }
}
