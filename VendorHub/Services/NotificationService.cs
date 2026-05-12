using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Hubs;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGeneralRepository<Notification> _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IGeneralRepository<Notification> notificationRepository, 
            IHubContext<NotificationHub> hubContext
            )
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        public async Task<GeneralResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository
                .GetAll()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(NotificationToDto())
                .ToListAsync();

            return new GeneralResponse<IEnumerable<NotificationDto>>().Succeeded(notifications);
        }

        public async Task<GeneralResponse> MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return new GeneralResponse().Failed("Notification not found");

            notification.IsRead = true;
            await _notificationRepository.SaveAsync();

            return new GeneralResponse().Succeeded("Notification marked as read");
        }

        private static Expression<Func<Notification, NotificationDto>> NotificationToDto()
        {
            return n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            };
        }


        #region newMethods
        public async Task SendOrderStatusNotificationAsync(int customerId, int orderId, string newStatus, string message)
        {
            var notification = new Notification
            {
                Title = "Order Status Updated",
                Message = message,
                Type = NotificationType.OrderStatusChanged,
                IsRead = false,
                UserId = customerId,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();

            await SendRealTimeNotification(customerId, notification);
        }


        private async Task SendRealTimeNotification(int customerId, Notification notification)
        {
            await _hubContext.Clients
                .Group($"user-{customerId}")
                .SendAsync("ReceiveNotification", new
                {
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type.ToString(),
                    OrderId = notification.OrderId,
                    CreatedAt = notification.CreatedAt
                });
        }
        #endregion
    }
}
