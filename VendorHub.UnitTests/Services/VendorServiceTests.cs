using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using VendorHub.DTOs.VendorDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class VendorServiceTests
    {
        private readonly Mock<IGeneralRepository<Vendor>> _vendorRepositoryMock;
        private readonly Mock<ILogger<VendorService>> _loggerMock;

        public VendorServiceTests()
        {
            _vendorRepositoryMock = new Mock<IGeneralRepository<Vendor>>();
            _loggerMock = new Mock<ILogger<VendorService>>();
        }

        private VendorService CreateSut()
        {
            return new VendorService(_vendorRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetVendorProfileAsync_WhenVendorNotFound_ReturnsNotFound()
        {
            // Arrange
            const int vendorId = 99;

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vendor?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.GetVendorProfileAsync(vendorId);

            // Assert
            result.ShouldBeNotFound();
        }

        [Fact]
        public async Task GetVendorProfileAsync_WhenVendorExists_ReturnsProfileDto()
        {
            // Arrange
            const int vendorId = 5;
            var vendor = new Vendor
            {
                Id = vendorId,
                Email = "vendor@store.com",
                FirstName = "Alex",
                SecondName = "Tech",
                PhoneNumber = "01099887766",
                StoreName = "Tech World",
                Balance = 1500,
                AccountStatus = AccountStatus.ACTIVE
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            var result = await sut.GetVendorProfileAsync(vendorId);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.Id.Should().Be(vendorId);
            data.Email.Should().Be("vendor@store.com");
            data.StoreName.Should().Be("Tech World");
            data.Balance.Should().Be(1500);
            data.Role.Should().Be("Vendor");
        }

        [Fact]
        public async Task UpdateVendorProfileAsync_WhenVendorNotFound_ReturnsNotFound()
        {
            // Arrange
            const int vendorId = 99;
            var dto = new UpdateVendorProfileDto
            {
                FirstName = "NewFirst",
                SecondName = "NewSecond",
                PhoneNumber = "01234567890",
                StoreName = "New Store"
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vendor?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateVendorProfileAsync(vendorId, dto);

            // Assert
            result.ShouldBeNotFound();
            _vendorRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateVendorProfileAsync_WhenValid_UpdatesFieldsAndReturnsSucceeded()
        {
            // Arrange
            const int vendorId = 5;
            var vendor = new Vendor
            {
                Id = vendorId,
                FirstName = "OldFirst",
                SecondName = "OldSecond",
                PhoneNumber = "01000000000",
                StoreName = "Old Store",
                Balance = 500,
                AccountStatus = AccountStatus.ACTIVE
            };

            var dto = new UpdateVendorProfileDto
            {
                FirstName = "Super",
                SecondName = "Vendor",
                PhoneNumber = "01122334455",
                StoreName = "Super Store"
            };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateVendorProfileAsync(vendorId, dto);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.FirstName.Should().Be("Super");
            data.SecondName.Should().Be("Vendor");
            data.StoreName.Should().Be("Super Store");

            vendor.StoreName.Should().Be("Super Store");

            _vendorRepositoryMock.Verify(r => r.Update(vendor), Times.Once);
            _vendorRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateVendorProfileAsync_WhenDbUpdateConcurrencyOccurs_ReturnsError()
        {
            // Arrange
            const int vendorId = 5;
            var vendor = new Vendor { Id = vendorId, StoreName = "Store" };

            _vendorRepositoryMock
                .Setup(r => r.GetByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vendor);

            _vendorRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateVendorProfileAsync(vendorId, new UpdateVendorProfileDto { StoreName = "New" });

            // Assert
            result.ShouldBeError();
        }

        [Fact]
        public async Task GetAllVendorsAsync_WhenInvoked_ReturnsPagedVendorDetails()
        {
            // Arrange
            var vendors = new List<Vendor>
            {
                new() { Id = 1, FirstName = "V1", SecondName = "S1", StoreName = "Store 1", Products = new List<Product> { new() } },
                new() { Id = 2, FirstName = "V2", SecondName = "S2", StoreName = "Store 2", Products = new List<Product>() }
            };

            _vendorRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(vendors.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllVendorsAsync(page: 1, pageSize: 10);

            // Assert
            var paged = result.ShouldBeSucceeded();
            paged.TotalCount.Should().Be(2);
            paged.Items.Should().NotBeNull();
            paged.Items!.Should().HaveCount(2);
            paged.Items.First().Id.Should().Be(2); 
        }
    }
}
