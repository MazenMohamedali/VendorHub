using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.VendorDto;
using VendorHub.DTOs.Vendors;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorProfileDto>>> GetVendorProfile(CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _vendorService.GetVendorProfileAsync(userId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorProfileDto>>> UpdateVendorProfile(UpdateVendorProfileDto dto, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _vendorService.UpdateVendorProfileAsync(userId, dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse<PagedResult<VendorDetailsDto>>>> GetVendors(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var response = await _vendorService.GetAllVendorsAsync(page, pageSize, cancellationToken);
            return this.HandleResult(response);
        }
    }
}
