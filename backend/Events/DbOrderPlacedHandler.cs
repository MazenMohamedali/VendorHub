using VendorHub.Models;
using VendorHub.Repository;
namespace VendorHub.Events
{
    public class DbOrderPlacedHandler : ICustomEventHandler<OrderPlacedEvent>
    {
        private readonly IGeneralRepository<Notification> _notificationRepository;

        public DbOrderPlacedHandler(IGeneralRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task HandleAsync(OrderPlacedEvent evnt)
        {
            var order = evnt.Order;

            var notifications = evnt.VendorSummaries.Select(summary => new Notification
            {
                UserId = summary.VendorId,
                OrderId = evnt.Order.Id,
                Type = NotificationType.NewPurchase,
                Data = new Dictionary<string, object>
                {
                    ["ItemsCount"] = summary.TotalItemsCount,
                    ["Earnings"] = summary.Subtotal
                }
            }).ToList();

            await _notificationRepository.AddRangeAsync(notifications); 
            await _notificationRepository.SaveAsync();
        }
    }
}
