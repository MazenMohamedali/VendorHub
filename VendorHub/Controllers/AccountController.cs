using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;
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


        #region Registration & Auth
        [HttpPost("register/customer")]
        public async Task<ActionResult<GeneralResponse>> RegisterCustomer(RegisterCustomerDto dto)
        {
            var result = await _accountService.RegisterCustomerAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        

        [HttpPost("register/vendor")]
        public async Task<IActionResult> RegisterVendor(RegisterVendorDto dto)
        {
            var result = await _accountService.RegisterVendorAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterUserDto dto)
        {
            var result = await _accountService.RegisterAdminAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _accountService.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(result);
            return Ok(result);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _accountService.LogoutAsync();
            return Ok(result);
        }
        #endregion

        #region User Profile
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyDetails()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.GetUserDetailsAsync(int.Parse(userId));
            return result.Success ? Ok(result) : NotFound(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(int.Parse(userId), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        #endregion

        #region Admin Actions
        [Authorize(Roles = "Admin")]
        [HttpPatch("approve-vendor/{id}")]
        public async Task<IActionResult> ApproveVendor(int id)
        {
            var result = await _accountService.ApproveVendorAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("reject-vendor/{id}")]
        public async Task<IActionResult> RejectVendor(int id)
        {
            var result = await _accountService.RejectVendorAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var result = await _accountService.DeactivateAccountAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        #endregion
    }
}
