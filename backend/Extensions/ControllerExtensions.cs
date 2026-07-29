using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VendorHub.Extensions
{
    public static class ControllerExtensions
    {
        public static int GetUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(userIdClaim))
                throw new InvalidOperationException("User is not authenticated.");
            return int.Parse(userIdClaim);
        }
    }
}
