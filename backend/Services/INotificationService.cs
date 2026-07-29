using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface INotificationService
    {
        Task<GeneralResponse<PagedResult<NotificationDto>>> GetAllNotificationsPagedAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<GeneralResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(int userId, CancellationToken cancellationToken = default);

        Task<GeneralResponse> DeleteNotificationAsync(int notificationId, int userId, CancellationToken cancellationToken = default);

        Task<GeneralResponse> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);

        Task<GeneralResponse> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);

    }
}
