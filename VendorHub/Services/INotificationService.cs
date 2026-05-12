using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface INotificationService
    {
        Task<GeneralResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(int userId);
        Task SendOrderStatusNotificationAsync(int customerId, int orderId, string newStatus, string message);
        Task<GeneralResponse> MarkAsReadAsync(int notificationId);
    }
}