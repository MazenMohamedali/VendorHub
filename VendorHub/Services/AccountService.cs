using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Settings;

namespace VendorHub.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _jwtOptions;
        private readonly IGeneralRepository<Customer> _customerRepository;
        private readonly IGeneralRepository<Vendor> _vendorRepository;
        private readonly IGeneralRepository<User> _userRepository;
        private readonly IPermissionService _permissionService;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration, 
            IOptions<JwtOptions> options,
            IGeneralRepository<Customer> customerRepository,
            IGeneralRepository<Vendor> vendorRepository,
            IGeneralRepository<User> userRepository,
            IPermissionService permissionService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _jwtOptions = options.Value;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
        }


        #region newMethods
        public async Task<GeneralResponse<ProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new GeneralResponse<ProfileDto>().Failed("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Unknown";

            var profile = await BuildProfileDto(user, role);

            return new GeneralResponse<ProfileDto>().Succeeded(profile, "Profile retrieved successfully");
        }

        public async Task<GeneralResponse<ProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new GeneralResponse<ProfileDto>().Failed("User not found");
            }

            if (!IsValidPhoneNumber(dto.PhoneNumber))
            {
                return new GeneralResponse<ProfileDto>().Failed("Invalid phone number");
            }

            UpdateUserBasicInfo(user, dto);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Unknown";

            await UpdateRoleSpecificInfo(userId, role, dto);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            var profile = await BuildProfileDto(user, role);

            return new GeneralResponse<ProfileDto>().Succeeded(profile, "Profile updated successfully");
        }

        private void UpdateUserBasicInfo(User user, UpdateProfileDto dto)
        {
            user.FirstName = dto.FirstName?.Trim() ?? user.FirstName;
            user.SecondName = dto.SecondName?.Trim() ?? user.SecondName;
            user.PhoneNumber = dto.PhoneNumber?.Trim();
        }

        private async Task UpdateRoleSpecificInfo(int userId, string role, UpdateProfileDto dto)
        {
            if (role == "Vendor" && !string.IsNullOrWhiteSpace(dto.StoreName))
            {
                var vendor = await _vendorRepository.GetByIdAsync(userId);
                if (vendor != null)
                {
                    vendor.StoreName = dto.StoreName.Trim();
                    await _vendorRepository.UpdateAsync(vendor);
                }
                return;
            }
            
            if (role == "Customer" && !string.IsNullOrWhiteSpace(dto.Address))
            {
                var customer = await _customerRepository.GetByIdAsync(userId);
                if (customer != null)
                {
                    customer.Address = dto.Address.Trim();
                    await _customerRepository.UpdateAsync(customer);
                }
            }
        }

        private async Task<ProfileDto> BuildProfileDto(User user, string role)
        {
            var dto = new ProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                AccountStatus = user.AccountStatus.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            if (role == "Vendor")
            {
                var vendor = await _vendorRepository.GetByIdAsync(user.Id);
                if (vendor != null)
                {
                    dto.StoreName = vendor.StoreName;
                    dto.Balance = vendor.Balance;
                }
                return dto;
            }
            
            if (role == "Customer")
            {
                var customer = await _customerRepository.GetByIdAsync(user.Id);
                if (customer != null)
                    dto.Address = customer.Address;
            }

            return dto;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("+", "");
            return cleaned.All(char.IsDigit) && phoneNumber.Length >= 7 && phoneNumber.Length <= 15;
        }
        #endregion

        #region Registeration Methods
        public async Task<GeneralResponse> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            var customer = new Customer { Address = dto.Address };
            RegisterUserDtoToUser(customer, dto);

            var result = await CreateAndAssignRole(customer, "Customer", dto.Password);
            return HandleIdentityResult(result, "Customer registered successfully");
        }

        public async Task<GeneralResponse> RegisterVendorAsync(RegisterVendorDto dto)
        {
            var vendor = new Vendor { StoreName = dto.StoreName };
            RegisterUserDtoToUser(vendor, dto);
            vendor.AccountStatus = AccountStatus.PENDING; 

            var result = await CreateAndAssignRole(vendor, "Vendor", dto.Password);
            return HandleIdentityResult(result, "Vendor registered successfully");
        }

        public async Task<GeneralResponse> RegisterAdminAsync(RegisterUserDto dto)
        {
            var admin = new Admin();
            RegisterUserDtoToUser(admin, dto);

            var result = await CreateAndAssignRole(admin, "Admin", dto.Password);
            return HandleIdentityResult(result, "Admin registered successfully");
        }

        public async Task CreateFirstAdminAsync(string firstName, string secondName, string email, string password, string phone)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Any()) return;

            var admin = new Admin
            {
                FirstName = firstName,
                SecondName = secondName,
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                AccountStatus = AccountStatus.ACTIVE
            };

            var result = await _userManager.CreateAsync(admin, password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "Admin");
        }
        #endregion

        #region Login&Auth Methods
        public async Task<GeneralResponse<string>> LoginAsync(LoginDto dto)
        {

            User? user = await _userManager.FindByEmailAsync(dto.Email);
            var response = await ValidationUser(user);
            if (!response.Success) return response;


            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
                return FailedSignIn(response, signInResult);

            var token = await GenerateJwtTokenAsync(user);
            return response.Succeeded(token, "Login successful");
        }
        private static GeneralResponse<string> FailedSignIn(GeneralResponse<string> response, SignInResult signInResult)
        {
            if (signInResult.IsLockedOut)
                return response.Failed("Account locked. Try again later");

            return response.Failed("Invalid email or password");
        }
        private async Task<GeneralResponse<string>> ValidationUser(User? user)
        {
            var result = new GeneralResponse<string>();

            if (user == null)
                return result.Failed("Invalid email or password");

            if (user.AccountStatus == AccountStatus.DELETED)
                return result.Failed("Account is inactive");

            if (user is Vendor && user.AccountStatus == AccountStatus.PENDING)
                return result.Failed("Your account is pending admin approval");

            return result.Succeeded("", "");
        }

        #region Jwt Generation
        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var userClaims = GetClaims(user);
            var credentials = GetCredentials();

            SecurityToken token = new JwtSecurityToken(
                    issuer: _jwtOptions.IssuerIP,
                    audience: _jwtOptions.AudienceIP,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: await credentials,
                    claims: await userClaims
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IEnumerable<Claim>> GetClaims(User user)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.SecondName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
                userClaims.Add(new Claim(ClaimTypes.Role, roleName));

            return userClaims;
        }

        public async Task<SigningCredentials> GetCredentials()
        {
            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.SecritKey ?? "")
                );
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        #endregion

        public async Task<GeneralResponse> LogoutAsync()
        {
            return new GeneralResponse { Success = true, Message = "Logged out successfully" };
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) == null;
        }

        #endregion

        #region Account Management Methods
        public async Task<GeneralResponse<UserDetailsDto>> GetUserDetailsAsync(int userId)
        {
            var response = new GeneralResponse<UserDetailsDto>();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return response.Failed("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            var details = new UserDetailsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccountStatus = user.AccountStatus,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
            };

            return response.Succeeded(details, "User details retrieved successfully");
        }

        public async Task<GeneralResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return new GeneralResponse().Failed("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            return HandleIdentityResult(result, "Password changed successfully");
        }

        public async Task<GeneralResponse> ApproveVendorAsync(int vendorId)
        {
            var user = await _userManager.FindByIdAsync(vendorId.ToString());
            if (user == null || user is not Vendor)
                return new GeneralResponse().Failed("Vendor not found");

            user.AccountStatus = AccountStatus.ACTIVE;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Grant default permissions
                await _permissionService.EnablePermissionForVendorAsync(vendorId, PermissionType.CanViewProducts);
                await _permissionService.EnablePermissionForVendorAsync(vendorId, PermissionType.CanViewOrders);
                await _permissionService.EnablePermissionForVendorAsync(vendorId, PermissionType.CanUpdateOrderStatus);
                await _permissionService.EnablePermissionForVendorAsync(vendorId, PermissionType.CanUploadProducts);
                // Add more as needed
            }

            return HandleIdentityResult(result, "Vendor approved successfully");
        }

        public async Task<GeneralResponse> RejectVendorAsync(int vendorId)
        {
            var user = await _userManager.FindByIdAsync(vendorId.ToString());

            if (user == null || user is not Vendor)
                return new GeneralResponse().Failed("Vendor not found");

            user.AccountStatus = AccountStatus.DELETED;
            var result = await _userManager.UpdateAsync(user);

            return HandleIdentityResult(result, $"Vendor rejected.");
        }

        public async Task<GeneralResponse> DeactivateAccountAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new GeneralResponse().Failed("User not found");

            user.AccountStatus = AccountStatus.DELETED;
            var result = await _userManager.UpdateAsync(user);

            return HandleIdentityResult(result, "Account deactivated successfully");
        }
        #endregion

        #region Private Helpers
        private void RegisterUserDtoToUser(User user, RegisterUserDto vm)
        {
            user.FirstName = vm.FirstName;
            user.SecondName = vm.SecondName;
            user.UserName = vm.Email;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.AccountStatus = AccountStatus.ACTIVE;
        }

        private async Task<IdentityResult> CreateAndAssignRole(User user, string role, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, role);
            return result;
        }

        private GeneralResponse HandleIdentityResult(IdentityResult result, string successMessage)
        {
            if (result.Succeeded)
            {
                return new GeneralResponse { Success = true, Message = successMessage };
            }

            return new GeneralResponse
            {
                Success = false,
                Message = "Operation failed",
                Errors = result.Errors.Select(e => new ValidationError
                {
                    Field = e.Code,
                    Message = e.Description
                }).ToList()
            };
        }
        #endregion
    }
}
