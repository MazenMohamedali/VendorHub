using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        #region Registration & Authentication
        [HttpPost("register/customer")]
        public async Task<ActionResult<GeneralResponse>> RegisterCustomer(RegisterCustomerDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.RegisterCustomerAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }   

        [HttpPost("register/vendor")]
        public async Task<ActionResult<GeneralResponse>> RegisterVendor(RegisterVendorDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.RegisterVendorAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register/admin")]
        public async Task<ActionResult<GeneralResponse>> RegisterAdmin(RegisterUserDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.RegisterAdminAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<GeneralResponse<string>>> Login(LoginDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.LoginAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<GeneralResponse>> Logout(CancellationToken cancellationToken = default)
        {
            var result = await _accountService.LogoutAsync(cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Identity Session
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<GeneralResponse<CurrentUserDto>>> GetCurrentIdentity(CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _accountService.GetCurrentIdentityAsync(userId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<GeneralResponse>> ChangePassword(ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();
            var result = await _accountService.ChangePasswordAsync(userId, dto, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Admin Actions
        [Authorize(Roles = "Admin")]
        [HttpPatch("approve-vendor/{id:int}")]
        public async Task<ActionResult<GeneralResponse>> ApproveVendor(int id, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.ApproveVendorAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("reject-vendor/{id:int}")]
        public async Task<ActionResult<GeneralResponse>> RejectVendor(int id, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.RejectVendorAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deactivate/{id:int}")]
        public async Task<ActionResult<GeneralResponse>> DeactivateUser(int id, CancellationToken cancellationToken = default)
        {
            var result = await _accountService.DeactivateAccountAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion
    }
}
