using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.UnitTests.Extensions;
using System.Linq.Expressions;
using MockQueryable;

namespace VendorHub.UnitTests.Services
{
    public class PermissionServiceTests
    {
        private readonly Mock<IGeneralRepository<Vendor>> _vendorRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<PermissionService>> _loggerMock;

        public PermissionServiceTests()
        {
            _vendorRepositoryMock = new Mock<IGeneralRepository<Vendor>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<PermissionService>>();
        }

        private PermissionService CreateSut()
        {
            return new PermissionService(
                _vendorRepositoryMock.Object,
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllPermissionsAsync_WhenInvoked_ReturnsActivePermissionsExcludingRestrictedFlags()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetAllPermissionsAsync(CancellationToken.None);

            // Assert
            var data = result.ShouldBeSucceeded();
            var systemNames = data.Select(p => p.SystemName).ToList();

            systemNames.Should().NotContain(nameof(PermissionType.None));
            systemNames.Should().NotContain(nameof(PermissionType.VendorAdmin));
            systemNames.Should().NotContain(nameof(PermissionType.VendorStaff));
        }

        [Fact]
        public async Task HasPermissionAsync_WhenVendorHasPermission_ReturnTrue()
        {
            // Arrange
            var vendor = new Vendor
            {
                Id = 1,
                Permission = PermissionType.CanViewProducts | PermissionType.CanUploadProducts
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Vendor, bool>>>()))
                .Returns(new List<Vendor> { vendor }.BuildMock());

            var sut = CreateSut();

            // Act
            var hasUploadPermission = await sut.HasPermissionAsync(1, PermissionType.CanUploadProducts);
            var hasOrderPermission = await sut.HasPermissionAsync(1, PermissionType.CanViewOrders);

            // Assert
            hasUploadPermission.Should().BeTrue();
            hasOrderPermission.Should().BeFalse();
        }

        [Theory]
        [InlineData(PermissionType.None)]
        [InlineData(PermissionType.VendorAdmin)]
        [InlineData(PermissionType.VendorStaff)]
        public async Task EnablePermissionForVendorAsync_WhenFlagIsRestricted_ReturnsInvalidInput(PermissionType restrictedType)
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.EnablePermissionForVendorAsync(vendorId: 1, restrictedType);

            // Assert
            result.ShouldBeInvalidInput();
            _vendorRepositoryMock.VerifyNoDatabaseMutations();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task EnablePermissionForVendorAsync_WhenVendorNotFound_ReturnsNotFound()
        {
            // Arrange
            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vendor?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.EnablePermissionForVendorAsync(vendorId: 1, PermissionType.CanUploadProducts);

            // Assert
            result.ShouldBeNotFound();
            _vendorRepositoryMock.VerifyNoDatabaseMutations();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task EnablePermissionForVendorAsync_WhenValid_AppliesBitwiseOrUpdatesDbAndEvictsCache()
        {
            // Arrange
            const int vendorId = 5;
            var vendor = new Vendor
            {
                Id = vendorId,
                Permission = PermissionType.CanViewProducts
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            var result = await sut.EnablePermissionForVendorAsync(vendorId, PermissionType.CanUploadProducts);

            // Assert
            result.ShouldBeSucceeded();
            vendor.Permission.HasFlag(PermissionType.CanViewProducts).Should().BeTrue();
            vendor.Permission.HasFlag(PermissionType.CanUploadProducts).Should().BeTrue();

            _vendorRepositoryMock.Verify(r => r.Update(vendor), Times.Once);
            _vendorRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.VendorPermissions(vendorId));
        }

        [Fact]
        public async Task DisablePermissionForVendorAsync_WhenValid_AppliesBitwiseAndNotUpdatesDbAndEvictsCache()
        {
            // Arrange
            const int vendorId = 5;
            var vendor = new Vendor
            {
                Id = vendorId,
                Permission = PermissionType.CanViewProducts | PermissionType.CanUploadProducts
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            var result = await sut.DisablePermissionForVendorAsync(vendorId, PermissionType.CanUploadProducts);

            // Assert
            result.ShouldBeSucceeded();
            vendor.Permission.HasFlag(PermissionType.CanViewProducts).Should().BeTrue();
            vendor.Permission.HasFlag(PermissionType.CanUploadProducts).Should().BeFalse();

            _vendorRepositoryMock.Verify(r => r.Update(vendor), Times.Once);
            _vendorRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.VendorPermissions(vendorId));
        }

        [Fact]
        public async Task GetVendorPermissionsAsync_WhenInvoked_ReturnsAllPermissionsWithCorrectEnabledStatus()
        {
            // Arrange
            const int vendorId = 3;
            var vendor = new Vendor
            {
                Id = vendorId,
                Permission = PermissionType.CanViewProducts | PermissionType.CanUploadProducts
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Vendor, bool>>>()))
                .Returns(new List<Vendor> { vendor }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetVendorPermissionsAsync(vendorId);

            // Assert
            var data = result.ShouldBeSucceeded().ToList();
            data.First(p => p.SystemName == nameof(PermissionType.CanViewProducts)).IsEnabled.Should().BeTrue();
            data.First(p => p.SystemName == nameof(PermissionType.CanUploadProducts)).IsEnabled.Should().BeTrue();
            data.First(p => p.SystemName == nameof(PermissionType.CanViewOrders)).IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task EnablePermissionForVendorAsync_WhenDbUpdateConcurrencyOccurs_ReturnsErrorAndDoesNotEvictCache()
        {
            // Arrange
            const int vendorId = 4;
            var vendor = new Vendor { Id = vendorId, Permission = PermissionType.CanViewProducts };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            _vendorRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.EnablePermissionForVendorAsync(vendorId, PermissionType.CanUploadProducts);

            // Assert
            result.ShouldBeError();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task AssignDefaultVendorPermissionsAsync_WhenVendorExists_SetsDefaultBitmaskAndEvictsCache()
        {
            // Arrange
            const int vendorId = 8;
            var vendor = new Vendor
            {
                Id = vendorId,
                Permission = PermissionType.None
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            await sut.AssignDefaultVendorPermissionsAsync(vendorId);

            // Assert
            vendor.Permission.HasFlag(PermissionType.VendorStaff).Should().BeTrue();

            _vendorRepositoryMock.Verify(r => r.Update(vendor), Times.Once);
            _vendorRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.VendorPermissions(vendorId));
        }

        [Fact]
        public async Task AssignDefaultVendorPermissionsAsync_WhenVendorDoesNotExist_DoesNotMutateDbOrCache()
        {
            // Arrange
            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vendor?)null);

            var sut = CreateSut();

            // Act
            await sut.AssignDefaultVendorPermissionsAsync(vendorId: 99);

            // Assert
            _vendorRepositoryMock.VerifyNoDatabaseMutations();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }
    }
}
