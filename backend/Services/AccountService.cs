using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.UserDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Settings;

namespace VendorHub.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtOptions _jwtOptions;
        private readonly IGeneralRepository<User> _userRepository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtOptions> options,
            IGeneralRepository<User> userRepository,
            IPermissionService permissionService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtOptions = options.Value;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_jwtOptions.SecritKey) || _jwtOptions.SecritKey.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret Key must be configured and at least 256 bits (32 characters) long.");
            }
        }

        #region Session Identity
        public async Task<GeneralResponse<CurrentUserDto>> GetCurrentIdentityAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || user.AccountStatus == AccountStatus.DELETED)
            {
                _logger.LogWarningWithContext("Attempted to fetch identity for deleted/invalid user {UserId}", userId);
                return GeneralResponse<CurrentUserDto>.Unauthenticated("User session is invalid or disabled.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Customer";

            return GeneralResponse<CurrentUserDto>.Succeeded(
                new CurrentUserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    Role = primaryRole
                },
                "User session initialized.");
        }
        #endregion

        #region Registration Methods
        public async Task<GeneralResponse> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Customer registration attempted for {Email}", dto.Email);

            var customer = new Customer { Address = dto.Address };
            RegisterUserDtoToUser(customer, dto);
            customer.AccountStatus = AccountStatus.ACTIVE;

            var result = await CreateAndAssignRole(customer, "Customer", dto.Password);

            if (!result.Succeeded)
                _logger.LogWarningWithContext("Customer registration rejected by Identity policies", result.Errors, dto.Email);

            return HandleIdentityResult(result, "Customer registered successfully", isCreated: true);
        }

        public async Task<GeneralResponse> RegisterVendorAsync(RegisterVendorDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vendor registration attempted for {Email}, Store: {StoreName}", dto.Email, dto.StoreName);

            var vendor = new Vendor { StoreName = dto.StoreName };
            RegisterUserDtoToUser(vendor, dto);
            vendor.AccountStatus = AccountStatus.PENDING; 

            var result = await CreateAndAssignRole(vendor, "Vendor", dto.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarningWithContext("Vendor registration rejected by Identity policies", result.Errors, dto.Email);
            }

            return HandleIdentityResult(result, "Vendor registered successfully", isCreated: true);
        }

        public async Task<GeneralResponse> RegisterAdminAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing administrative account creation for email {Email}", dto.Email);

            var admin = new Admin();
            RegisterUserDtoToUser(admin, dto);

            var result = await CreateAndAssignRole(admin, "Admin", dto.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarningWithContext("Administrative registration rejected by Identity policies", result.Errors, dto.Email);
            }

            return HandleIdentityResult(result, "Admin registered successfully", isCreated: true);
        }

        public async Task CreateFirstAdminAsync(string firstName, string secondName, string email, string password, string phone, CancellationToken cancellationToken = default)
        {
            var existingAdmin = await _userManager.FindByEmailAsync(email);
            if (existingAdmin != null)
            {
                if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
                {
                    await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                }
                return;
            }

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
            if (!result.Succeeded)
            {
                _logger.LogWarning("Seed administrator account initialization notice for {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            var roleResult = await _userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                _logger.LogErrorWithContext("Failed to assign role to seed administrator account. Rolling back.", roleResult.Errors, email);
                await _userManager.DeleteAsync(admin);
                return;
            }

            _logger.LogInformation("Seed administrator account successfully configured.");
        }
        #endregion

        #region Login & Authentication Methods
        public async Task<GeneralResponse<string>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Authentication request received for account: {Email}", dto.Email);

            User? user = await _userManager.FindByEmailAsync(dto.Email);
            var response = await ValidateUserAsync(user);
            if (!response.Success) return response;

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user!, dto.Password, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Authentication failed for user {Email}. Status: {SignInStatus}", dto.Email, signInResult);
                return FailedSignIn(signInResult);
            }

            var token = await GenerateJwtTokenAsync(user!);
            _logger.LogInformation("User {Email} successfully authenticated. JWT dispatched.", dto.Email);

            return GeneralResponse<string>.Succeeded(token, "Login successful");
        }
        
        private static GeneralResponse<string> FailedSignIn(SignInResult signInResult)
        {
            if (signInResult.IsLockedOut)
                return GeneralResponse<string>.InvalidInput("Account is locked due to multiple failed login attempts.");

            return GeneralResponse<string>.InvalidInput("Invalid email or password.");
        }

        public async Task<GeneralResponse> LogoutAsync(CancellationToken cancellationToken = default)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out. Client should discard JWT.");
            return GeneralResponse.Succeeded("Logged out successfully");
        }

        private async Task<GeneralResponse<string>> ValidateUserAsync(User? user)
        {
            if (user == null)
                return GeneralResponse<string>.InvalidInput("Invalid email or password");

            if (user.AccountStatus == AccountStatus.DELETED)
            {
                _logger.LogWarning("Login rejected. User account {Email} has been marked as deleted.", user.Email);
                return GeneralResponse<string>.Forbidden("Account is inactive");
            }

            if (user is Vendor && user.AccountStatus == AccountStatus.PENDING)
            {
                _logger.LogWarning("Login rejected. Vendor account {Email} is awaiting admin authorization.", user.Email);
                return GeneralResponse<string>.Forbidden("Your account is pending admin approval");
            }

            return GeneralResponse<string>.Succeeded(string.Empty, "User state is valid.");
        }

        public async Task<GeneralResponse> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out. Client should discard JWT.");
            return GeneralResponse.Succeeded("Logged out successfully");
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userManager.FindByEmailAsync(email) == null;
        }
        #endregion

        #region Jwt Generation
        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var userClaims = await GetClaimsAsync(user);
            var credentials = GetCredentials();

            SecurityToken token = new JwtSecurityToken(
                    issuer: _jwtOptions.IssuerIP,
                    audience: _jwtOptions.AudienceIP,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: credentials,
                    claims: userClaims
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync(User user)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.SecondName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in userRoles)
                userClaims.Add(new Claim(ClaimTypes.Role, roleName));

            return userClaims;
        }

        public SigningCredentials GetCredentials()
        {
            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.SecritKey)
                );
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }
        #endregion

        #region Account Administration Management
        public async Task<GeneralResponse<UserDetailsDto>> GetUserDetailsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return GeneralResponse<UserDetailsDto>.NotFound("User not found");

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

            return GeneralResponse<UserDetailsDto>.Succeeded(details, "User details retrieved successfully");
        }

        public async Task<GeneralResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return GeneralResponse.NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            return HandleIdentityResult(result, "Password changed successfully");
        }

        public async Task<GeneralResponse> ApproveVendorAsync(int vendorId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin initializing vetting approval loop for Vendor ID: {VendorId}", vendorId);

            var user = await _userManager.FindByIdAsync(vendorId.ToString());
            if (user == null || user is not Vendor)
                return GeneralResponse.NotFound("Vendor not found");

            user.AccountStatus = AccountStatus.ACTIVE;
            var result = await _userManager.UpdateAsync
                (user);

            if(!result.Succeeded)
            {
                _logger.LogErrorWithContext("Failed to update Vendor status flag to Active.", result.Errors, vendorId);
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("Vendor identity updated to Active status. Assigning core permissions.");
                await _permissionService.AssignDefaultVendorPermissionsAsync(vendorId, cancellationToken);
            }

            return HandleIdentityResult(result, "Vendor approved successfully");
        }

        public async Task<GeneralResponse> RejectVendorAsync(int vendorId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(vendorId.ToString());
            if (user == null || user is not Vendor vendor)
                return GeneralResponse.NotFound("Vendor not found");

            vendor.AccountStatus = AccountStatus.DELETED;
            var result = await _userManager.UpdateAsync(vendor);

            return HandleIdentityResult(result, "Vendor rejected successfully");
        }

        public async Task<GeneralResponse> DeactivateAccountAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return GeneralResponse.NotFound("Account not found");

            _logger.LogWarning("Deactivating identity profile for user account: {Email}", user.Email);
            user.AccountStatus = AccountStatus.DELETED;

            var result = await _userManager.UpdateAsync(user);
            return HandleIdentityResult(result, "Account deactivated successfully");
        }

        public async Task<GeneralResponse> UpdateAccountStatusByConditionAsync(User? user, Predicate<User?> rejectedCondition, string successMessage, CancellationToken cancellationToken = default)
        {
            if (rejectedCondition(user))
                return GeneralResponse.NotFound("User not found or invalid");

            _logger.LogWarning("Deactivating identity profile for user account: {Email}", user!.Email);

            user!.AccountStatus = AccountStatus.DELETED;
            var result = await _userManager.UpdateAsync(user);

            return HandleIdentityResult(result, successMessage);
        }
        #endregion

        #region Private Mappers & Handlers
        private void RegisterUserDtoToUser(User user, RegisterUserDto vm)
        {
            user.FirstName = vm.FirstName.Trim();
            user.SecondName = vm.SecondName.Trim();
            user.UserName = vm.Email.Trim();
            user.Email = vm.Email.Trim();
            user.PhoneNumber = vm.PhoneNumber?.Trim();
        }

        private async Task<IdentityResult> CreateAndAssignRole(User user, string role, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return result;

            var roleResult = await _userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                _logger.LogErrorWithContext($"Failed to assign role {role}. Rolling back database write operation", roleResult.Errors, user.Email);
                await _userManager.DeleteAsync(user);
                return roleResult;
            }
            return result;
        }

        private GeneralResponse HandleIdentityResult(IdentityResult result, string successMessage, bool isCreated = false)
        {
            if (result.Succeeded)
                return isCreated ? GeneralResponse.Created(successMessage) : GeneralResponse.Succeeded(successMessage);

            var validationErrors = result.Errors.Select(e => new ValidationError
            {
                Field = e.Code,
                Message = e.Description
            }).ToList();

            return GeneralResponse.InvalidInput("One or more validation errors occurred.", validationErrors);
        }
        #endregion
    }
}
