// Controllers/PermissionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
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


        [HttpPost]
        public async Task<ActionResult<GeneralResponse<object>>> Create(CreatePermissionDto dto)
        {
            var result = await _permissionService.CreatePermissionAsync(dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        
        [HttpGet]
        public async Task<ActionResult<GeneralResponse<IEnumerable<PermissionDto>>>> GetAll()
        {
            var result = await _permissionService.GetAllPermissionsAsync();
            return Ok(result);
        }

        
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<VendorPermissionDto>>>> GetVendorPermissions(int vendorId)
        {
            var result = await _permissionService.GetVendorPermissionsAsync(vendorId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }


        [HttpPost("vendor/{vendorId}/enable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse<object>>> EnableForVendor(
            int vendorId,
            PermissionType permissionType)
        {
            var result = await _permissionService.EnablePermissionForVendorAsync(vendorId, permissionType);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("vendor/{vendorId}/disable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse<object>>> DisableForVendor(
            int vendorId,
            PermissionType permissionType)
        {
            var result = await _permissionService.DisablePermissionForVendorAsync(vendorId, permissionType);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("role/enable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse<object>>> EnableForRole(PermissionType permissionType)
        {
            var result = await _permissionService.EnablePermissionForVendorAsync(permissionType);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("role/disable/{permissionType}")]
        public async Task<ActionResult<GeneralResponse<object>>> DisableForRole(PermissionType permissionType)
        {
            var result = await _permissionService.DisablePermissionForVendorAsync(permissionType);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}