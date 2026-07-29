using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Events
{
    public class DbOrderStatusChangedHandler : ICustomEventHandler<OrderStatusChangedEvent>
    {
        private readonly IGeneralRepository<Notification> _notificationRepository;

        public DbOrderStatusChangedHandler(IGeneralRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task HandleAsync(OrderStatusChangedEvent evnt)
        {
            var notification = new Notification
            {
                UserId = evnt.CustomerId,
                OrderId = evnt.OrderId,
                Type = NotificationType.StatusUpdate,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    ["Status"] = evnt.NewStatus
                }
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();
        }

    }
}
