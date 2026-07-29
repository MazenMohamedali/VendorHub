using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<GeneralResponse<IEnumerable<PermissionDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.GetAllPermissionsAsync(cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("vendor/{vendorId:int}")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<VendorPermissionDto>>>> GetVendorPermissions(
            int vendorId,
            CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.GetVendorPermissionsAsync(vendorId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("vendor/{vendorId:int}/enable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse>> EnableForVendor(
            int vendorId,
            PermissionType permissionType,
            CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.EnablePermissionForVendorAsync(vendorId, permissionType, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("vendor/{vendorId:int}/disable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse>> DisableForVendor(
            int vendorId,
            PermissionType permissionType,
            CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.DisablePermissionForVendorAsync(vendorId, permissionType, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("global/enable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse>> EnableGlobally(
            PermissionType permissionType,
            CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.GlobalEnablePermissionAsync(permissionType, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("global/disable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse>> DisableGlobally(
            PermissionType permissionType,
            CancellationToken cancellationToken = default)
        {
            var result = await _permissionService.GlobalDisablePermissionAsync(permissionType, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
