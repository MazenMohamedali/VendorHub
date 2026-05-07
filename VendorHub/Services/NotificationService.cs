using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGeneralRepository<Notification> _notificationRepository;

        public NotificationService(IGeneralRepository<Notification> notificationRepository)
        {
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
    }
}
