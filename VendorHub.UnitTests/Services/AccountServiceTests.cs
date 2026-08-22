using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VendorHub.DTOs.UserDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Settings;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IGeneralRepository<User>> _userRepositoryMock;
        private readonly Mock<IPermissionService> _permissionServiceMock;
        private readonly Mock<ILogger<AccountService>> _loggerMock;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public AccountServiceTests()
        {
            _userManagerMock = MockUserManager<User>();
            _signInManagerMock = MockSignInManager(_userManagerMock);
            _userRepositoryMock = new Mock<IGeneralRepository<User>>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _loggerMock = new Mock<ILogger<AccountService>>();

            _jwtOptions = Options.Create(new JwtOptions
            {
                SecritKey = "ThisIsASecretKeyThatIsAtLeast32CharsLongForTesting!",
                IssuerIP = "https://vendorhub.com",
                AudienceIP = "https://vendorhub.com"
            });
        }
        private AccountService CreateSut(IOptions<JwtOptions>? options = null)
        {
            return new AccountService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                options ?? _jwtOptions,
                _userRepositoryMock.Object,
                _permissionServiceMock.Object,
                _loggerMock.Object
            );
        }


        public static TheoryData<string?> InvalidKeyData =>
            new()
            {
                null,
                string.Empty,
                new string(' ', 10),  // Whitespace < 32 chars
                new string(' ', 40),  // Whitespace > 32 chars
                new string('a', 1),   // Length 1
                new string('a', 31)   // Exact boundary: Length 31 (32 - 1)
            };

        [Theory]
        [MemberData(nameof(InvalidKeyData))]
        public void Constructor_InvalidJwtKey_ThrowsInvalidOperationException(string? invalidKey)
        {
            // Arrange
            var jwtOptions = Options.Create(new JwtOptions { SecritKey = invalidKey! });

            // Act
            var act = () => CreateSut(jwtOptions);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task GetCurrentIdentityAsync_UserNotFound_ReturnUnAuthenticated()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            var sut = CreateSut();

            // Act
            var result = await sut.GetCurrentIdentityAsync(1);

            // Assert
            result.ShouldBeUnauthenticated();
        }

        [Fact]
        public async Task GetCurrentIdentityAsync_UserIsDeleted_ReturnsUnauthenticated()
        {
            // Arrange
            var deletedUser = new User { Id = 1, AccountStatus = AccountStatus.DELETED };
            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedUser);
            var sut = CreateSut();

            // Act
            var result = await sut.GetCurrentIdentityAsync(1);

            // Assert
            result.ShouldBeUnauthenticated();
        }

        [Fact]
        public async Task RegisterCustomerAsync_WhenRolesAssignmentFails_RollsBackUserCreation()
        {
            // Arrange
            var dto = new RegisterCustomerDto
            {
                Email = "newuser@test.com",
                Password = "Password123!",
                FirstName = "Test",
                SecondName = "User",
                Address = "Cairo"
            };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Customer"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = "Role missing" }));

            var sut = CreateSut();

            // Act
            var result = await sut.RegisterCustomerAsync(dto);

            // Assert
            result.ShouldBeInvalidInput();
            _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_NonExistingUser_ReturnsInvalidInput()
        {
            // Arrange
            _userManagerMock
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.LoginAsync(new LoginDto { Email = "ghost@hub.com", Password = "Pass" });

            // Assert
            result.ShouldBeInvalidInput();
        }

        [Fact]
        public async Task LoginAsync_PendingVendor_ReturnsForbidden()
        {
            // Arrange
            var vendor = new Vendor { Email = "vendor@store.com", AccountStatus = AccountStatus.PENDING };
            _userManagerMock
                .Setup(m => m.FindByEmailAsync(vendor.Email))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            var result = await sut.LoginAsync(new LoginDto { Email = vendor.Email, Password = "Password123" });

            // Assert
            result.ShouldBeForbidden();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var user = new Customer
            {
                Id = 10,
                Email = "customer@hub.com",
                FirstName = "Ali",
                SecondName = "Hassan",
                AccountStatus = AccountStatus.ACTIVE
            };

            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

            _signInManagerMock
                .Setup(s => s.CheckPasswordSignInAsync(user, "ValidPass123!", true))
                .ReturnsAsync(SignInResult.Success);

            var sut = CreateSut();

            // Act
            var result = await sut.LoginAsync(new LoginDto { Email = user.Email, Password = "ValidPass123!" });

            // Assert
            var token = result.ShouldBeSucceeded();
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ApproveVendorAsync_WhenUserIsVendor_UpdatesStatusAndEnablesPermissions()
        {
            // Arrange
            var vendor = new Vendor { Id = 1, AccountStatus = AccountStatus.PENDING };
            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(vendor);
            _userManagerMock.Setup(m => m.UpdateAsync(vendor)).ReturnsAsync(IdentityResult.Success);

            var sut = CreateSut();

            // Act
            var result = await sut.ApproveVendorAsync(1);

            // Assert
            result.ShouldBeSucceeded();
            vendor.AccountStatus.Should().Be(AccountStatus.ACTIVE);

            _permissionServiceMock.Verify(
                    p => p.AssignDefaultVendorPermissionsAsync(1, It.IsAny<CancellationToken>()),
                    Times.Once
                );
        }

        [Fact]
        public async Task ApproveVendorAsync_WhenUserDoesNotExist_ReturnNotFound()
        {
            // Arrange
            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync((User?)null);
            var sut = CreateSut();

            // Act
            var result = await sut.ApproveVendorAsync(1);

            // Assert
            result.ShouldBeNotFound();
        }

        [Fact]
        public async Task ApproveVendorAsync_WhenUserIsNotVendor_ReturnNotFound()
        {
            // Arrange
            var customer = new Customer { Id = 1 };
            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(customer);
            var sut = CreateSut();

            // Act
            var result = await sut.ApproveVendorAsync(1);

            // Assert
            result.ShouldBeNotFound();

            _userManagerMock
                .Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
            _permissionServiceMock.Verify(
                p => p.AssignDefaultVendorPermissionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenAccountIsLockedOut_ReturnsLockoutMessage()
        {
            // Arrange
            var user = new Customer { Email = "locked@hub.com", AccountStatus = AccountStatus.ACTIVE };
            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _signInManagerMock
                .Setup(s => s.CheckPasswordSignInAsync(user, "WrongPass", true))
                .ReturnsAsync(SignInResult.LockedOut);

            var sut = CreateSut();

            // Act
            var result = await sut.LoginAsync(new LoginDto { Email = user.Email, Password = "WrongPass" });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("locked");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsIncorrect_ReturnsInvalidInput()
        {
            // Arrange
            var user = new Customer { Email = "user@hub.com", AccountStatus = AccountStatus.ACTIVE };
            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _signInManagerMock
                .Setup(s => s.CheckPasswordSignInAsync(user, "WrongPass", true))
                .ReturnsAsync(SignInResult.Failed);

            var sut = CreateSut();

            // Act
            var result = await sut.LoginAsync(new LoginDto { Email = user.Email, Password = "WrongPass" });

            // Assert
            result.ShouldBeInvalidInput();
        }

        [Fact]
        public async Task RegisterVendorAsync_SetsAccountStatusToPending()
        {
            // Arrange
            var dto = new RegisterVendorDto
            {
                Email = "vendor@store.com",
                Password = "Password123!",
                FirstName = "John",
                SecondName = "Doe",
                StoreName = "Tech Store"
            };

            Vendor? capturedVendor = null;
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
                .Callback<User, string>((u, _) => capturedVendor = u as Vendor)
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Vendor"))
                .ReturnsAsync(IdentityResult.Success);

            var sut = CreateSut();

            // Act
            var result = await sut.RegisterVendorAsync(dto);

            // Assert
            result.ShouldBeCreated();
            capturedVendor.Should().NotBeNull();
            capturedVendor!.AccountStatus.Should().Be(AccountStatus.PENDING);
            capturedVendor.StoreName.Should().Be("Tech Store");
        }

        #region Mock Helpers
        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<TUser>> MockSignInManager<TUser>(Mock<UserManager<TUser>> userManager) where TUser : class
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            return new Mock<SignInManager<TUser>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null!, null!, null!, null!); 
        }
        #endregion
    }
}
