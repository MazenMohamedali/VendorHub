using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Events;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGeneralRepository<Notification> _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IGeneralRepository<Notification> notificationRepository,
            IEventQueue<OrderStatusChangedEvent> statusChangedEventQueue,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<GeneralResponse<PagedResult<NotificationDto>>> GetAllNotificationsPagedAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _notificationRepository
                .GetAllAsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToPagedResultAsync(NotificationToDto(), pageNumber, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<NotificationDto>>.Succeeded(result, "Notification stream retrieved successfully.");
        }

        public async Task<GeneralResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var notifications = await _notificationRepository
                .GetAllAsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(NotificationToDto())
                .ToListAsync(cancellationToken);

            return GeneralResponse<IEnumerable<NotificationDto>>.Succeeded(notifications, "Unread notifications fetched successfully.");
        }

        public async Task<GeneralResponse> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    _logger.LogWarningWithContext("Notification mutation target not found. ID: {NotificationId}",
                        new { NotificationId = notificationId }, notificationId);
                    return GeneralResponse.NotFound("Notification not found");
                }

                if (notification.IsRead)
                {
                    return GeneralResponse.Succeeded("Notification is already marked as read.");
                }

                notification.IsRead = true;
                
                _notificationRepository.Update(notification);
                await _notificationRepository.SaveAsync(cancellationToken);

                _logger.LogInfoWithContext("Notification state marked as read successfully. ID: {NotificationId}",
                    new { NotificationId = notificationId }, notificationId);

                return GeneralResponse.Succeeded("Notification marked as read");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogErrorWithContext("Concurrency conflict while updating notification ID: {NotificationId}",
                    new { NotificationId = notificationId, Exception = ex.Message }, notificationId);
                return GeneralResponse.Error("The notification state was updated by another request. Please try again.");
            }
        }

        public async Task<GeneralResponse> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
        {
            var unreadNotifications = await _notificationRepository
                .GetAll()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            if (!unreadNotifications.Any())
                return GeneralResponse.Succeeded("No unread notifications found.");

            foreach (var notification in unreadNotifications)
                notification.IsRead = true;

            await _notificationRepository.SaveAsync(cancellationToken);

            _logger.LogInfoWithContext("All notifications marked as read successfully for User ID: {UserId}",
                new { UserId = userId, Count = unreadNotifications.Count }, userId);

            return GeneralResponse.Succeeded($"Successfully marked {unreadNotifications.Count} notifications as read.");
        }

        public async Task<GeneralResponse> DeleteNotificationAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
                return GeneralResponse.NotFound("Notification not found.");

            if (notification.UserId != userId)
            {
                _logger.LogWarningWithContext("Unauthorized notification deletion attempt. User ID: {UserId}, Target Notification ID: {NotificationId}",
                    new { UserId = userId, NotificationId = notificationId }, notificationId);
                return GeneralResponse.Forbidden("Access denied.");
            }

            _notificationRepository.Delete(notification);
            await _notificationRepository.SaveAsync(cancellationToken);

            _logger.LogInfoWithContext("Notification deleted successfully. ID: {NotificationId}",
                new { NotificationId = notificationId }, notificationId);

            return GeneralResponse.Succeeded("Notification removed successfully.");
        }

        private static Expression<Func<Notification, NotificationDto>> NotificationToDto()
        {
            return n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                OrderId = n.OrderId,
                ProductId = n.ProductId
            };
        }
    }
}
