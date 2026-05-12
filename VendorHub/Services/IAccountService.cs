using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;

namespace VendorHub.Services
{
    public interface IAccountService
    {
        Task<GeneralResponse> ApproveVendorAsync(int vendorId);
        Task<GeneralResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task CreateFirstAdminAsync(string firstName, string secondName, string email, string password, string phone);
        Task<GeneralResponse> DeactivateAccountAsync(int userId);
        Task<GeneralResponse<UserDetailsDto>> GetUserDetailsAsync(int userId);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<GeneralResponse<string>> LoginAsync(LoginDto dto);
        Task<GeneralResponse> LogoutAsync();
        Task<GeneralResponse> RegisterAdminAsync(RegisterUserDto dto);
        Task<GeneralResponse> RegisterCustomerAsync(RegisterCustomerDto dto);
        Task<GeneralResponse> RegisterVendorAsync(RegisterVendorDto dto);
        Task<GeneralResponse> RejectVendorAsync(int vendorId);
        Task<GeneralResponse<ProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<GeneralResponse<ProfileDto>> GetProfileAsync(int userId);  
    }
}