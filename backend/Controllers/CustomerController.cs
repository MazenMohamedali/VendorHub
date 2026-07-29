using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.CustomerDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<GeneralResponse<CustomerProfileDto>>> GetCustomerProfile(CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _customerService.GetCustomerProfileAsync(userId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("profile")]
        public async Task<ActionResult<GeneralResponse<CustomerProfileDto>>> UpdateCustomerProfile(UpdateCustomerProfileDto dto, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _customerService.UpdateCustomerProfileAsync(userId, dto, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
