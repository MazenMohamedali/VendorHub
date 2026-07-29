using Microsoft.AspNetCore.SignalR;
using VendorHub.Hubs;
using VendorHub.Models;

namespace VendorHub.Events
{
    public class SignalrOrderPlacedHandler : ICustomEventHandler<OrderPlacedEvent>
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public SignalrOrderPlacedHandler(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleAsync(OrderPlacedEvent evnt)
        {
            foreach (var summary in evnt.VendorSummaries)
            {
                var payload = new
                {
                    OrderId = evnt.Order.Id,
                    Type = NotificationType.NewPurchase,
                    ItemsCount = summary.TotalItemsCount,
                    Earnings = summary.Subtotal,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"vendor-{summary.VendorId}")
                    .ReceiveNewPurchaseNotification(payload);
            }
        }
    }
}
