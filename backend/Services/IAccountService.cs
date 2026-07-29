using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IAccountService
    {
        Task<GeneralResponse<CurrentUserDto>> GetCurrentIdentityAsync(int userId, CancellationToken cancellationToken = default);

        Task<GeneralResponse> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default);
        Task<GeneralResponse> RegisterVendorAsync(RegisterVendorDto dto, CancellationToken cancellationToken = default);
        Task<GeneralResponse> RegisterAdminAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
        Task CreateFirstAdminAsync(string firstName, string secondName, string email, string password, string phone, CancellationToken cancellationToken = default);

        Task<GeneralResponse<string>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
        Task<GeneralResponse> LogoutAsync(CancellationToken cancellationToken = default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<Claim>> GetClaimsAsync(User user);
        SigningCredentials GetCredentials();

        Task<GeneralResponse<UserDetailsDto>> GetUserDetailsAsync(int userId, CancellationToken cancellationToken = default);
        Task<GeneralResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
        Task<GeneralResponse> ApproveVendorAsync(int vendorId, CancellationToken cancellationToken = default);
        Task<GeneralResponse> RejectVendorAsync(int vendorId, CancellationToken cancellationToken = default);
        Task<GeneralResponse> DeactivateAccountAsync(int userId, CancellationToken cancellationToken = default);
        Task<GeneralResponse> UpdateAccountStatusByConditionAsync(User? user, Predicate<User?> rejectedCondition, string successMessage, CancellationToken cancellationToken = default);
    }
}
