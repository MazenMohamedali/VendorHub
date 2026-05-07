using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorHub.DTOs.sharedDto;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new GeneralResponse().Failed("User not authenticated"));

            int userId = int.Parse(userIdClaim);
            var result = await _notificationService.GetUnreadNotificationsAsync(userId);

            return Ok(result);
        }


        [HttpPut("{notificationId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);

            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
