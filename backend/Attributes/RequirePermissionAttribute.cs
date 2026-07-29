using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using VendorHub.Models;
using VendorHub.Services;

namespace VendorHub.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly PermissionType _permissionType;
        public RequirePermissionAttribute(PermissionType permissionType)
        {
            _permissionType = permissionType;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (userRole == "Admin")
                return;

            if (userRole != "Vendor")
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

            if (!int.TryParse(userId, out int vendorId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasPermission = await permissionService
                .HasPermissionAsync(vendorId, _permissionType);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
