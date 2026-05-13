using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.Vendors;
using VendorHub.DTOs.sharedDto;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<GeneralResponse<IEnumerable<VendorDetailsDto>>>> GetVendors()
        {
            var response = await _vendorService.GetAllVendorsAsync();
            return Ok(response);
        }
    }
}