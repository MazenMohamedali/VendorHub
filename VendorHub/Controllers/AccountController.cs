using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        #region newMethods
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<GeneralResponse<ProfileDto>>> GetProfile()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _accountService.GetProfileAsync(userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<ActionResult<GeneralResponse<ProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsValidInput(dto))
                return BadRequest("Invalid input");

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _accountService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update-address")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<GeneralResponse<ProfileDto>>> UpdateCustomerAddress([FromBody] UpdateAddressDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Address) || dto.Address.Length < 5)
                return BadRequest("Address must be at least 5 characters");

            var userId = GetUserId();

            var currentProfile = await _accountService.GetProfileAsync(userId);
            if (!currentProfile.Success)
                return NotFound(currentProfile);

            var updateDto = new UpdateProfileDto
            {
                FirstName = currentProfile.Data.FirstName,
                SecondName = currentProfile.Data.SecondName,
                PhoneNumber = currentProfile.Data.PhoneNumber,
                Address = dto.Address
            };

            var result = await _accountService.UpdateProfileAsync(userId, updateDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update-store-name")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<ProfileDto>>> UpdateVendorStoreName([FromBody] UpdateStoreNameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.StoreName) || dto.StoreName.Length < 3)
                return BadRequest("Store name must be at least 3 characters");

            var userId = GetUserId();

            var currentProfile = await _accountService.GetProfileAsync(userId);
            if (!currentProfile.Success)
                return NotFound(currentProfile);

            var updateDto = new UpdateProfileDto
            {
                FirstName = currentProfile.Data.FirstName,
                SecondName = currentProfile.Data.SecondName,
                PhoneNumber = currentProfile.Data.PhoneNumber,
                StoreName = dto.StoreName
            };

            var result = await _accountService.UpdateProfileAsync(userId, updateDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private bool IsValidInput(UpdateProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || dto.FirstName.Length < 2)
                return false;

            if (string.IsNullOrWhiteSpace(dto.SecondName) || dto.SecondName.Length < 2)
                return false;

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return false;

            return true;
        }
    
        #endregion

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
