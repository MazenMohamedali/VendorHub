using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.NotificationDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Authorize(Roles = "Customer,Vendor")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<GeneralResponse<PagedResult<NotificationDto>>>> GetHistory(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _notificationService.GetAllNotificationsPagedAsync(userId, pageNumber, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("unread")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<NotificationDto>>>> GetUnread(CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _notificationService.GetUnreadNotificationsAsync(userId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("{notificationId}/mark-read")]
        public async Task<ActionResult<GeneralResponse>> MarkAsRead(int notificationId, CancellationToken cancellationToken = default)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("mark-all-read")]
        public async Task<ActionResult<GeneralResponse>> MarkAllAsRead(CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpDelete("{notificationId:int}")]
        public async Task<ActionResult<GeneralResponse>> DeleteNotification(int notificationId, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _notificationService.DeleteNotificationAsync(notificationId, userId, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
