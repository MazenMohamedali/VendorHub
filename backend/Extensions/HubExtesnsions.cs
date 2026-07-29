using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace VendorHub.Extensions
{
    public static class HubExtesnsions
    {
        public static int GetUserId(this Hub hub)
        {
            return int.Parse(hub.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        public static string GetUserRole(this Hub hub)
        {
            return (hub.Context.User?.FindFirst(ClaimTypes.Role)?.Value).ToString();
        }

        public static string GetUserName(this Hub hub)
        {
            return (hub.Context.User?.FindFirst(ClaimTypes.Name)?.Value).ToString();
        }
    }
}
