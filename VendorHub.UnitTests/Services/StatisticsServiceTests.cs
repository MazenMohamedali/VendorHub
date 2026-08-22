using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using VendorHub.DTOs.StatisticsDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class StatisticsServiceTests
    {
        private readonly Mock<IGeneralRepository<OrderItem>> _orderItemRepositoryMock;
        private readonly Mock<IGeneralRepository<Product>> _productRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<StatisticsService>> _loggerMock;

        public StatisticsServiceTests()
        {
            _orderItemRepositoryMock = new Mock<IGeneralRepository<OrderItem>>();
            _productRepositoryMock = new Mock<IGeneralRepository<Product>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<StatisticsService>>();
        }

        private StatisticsService CreateSut()
        {
            return new StatisticsService(
                _orderItemRepositoryMock.Object,
                _productRepositoryMock.Object,
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetVendorStatisticsAsync_WhenCached_ReturnsCachedStatistics()
        {
            // Arrange
            const int vendorId = 5;
            var cachedStats = new VendorStatisticsDto
            {
                TotalSales = 100,
                TotalRevenue = 5000,
                TotalOrders = 20,
                TotalProducts = 10,
                AverageProductRating = 4.8
            };

            _cacheServiceMock
                .Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<VendorStatisticsDto>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedStats);

            var sut = CreateSut();

            // Act
            var result = await sut.GetVendorStatisticsAsync(vendorId, CancellationToken.None);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.TotalSales.Should().Be(100);
            data.TotalRevenue.Should().Be(5000);
            data.TotalOrders.Should().Be(20);
        }

        [Fact]
        public async Task GetVendorStatisticsAsync_WhenCacheMiss_ComputesKpisAndReturnsCompiledMetrics()
        {
            // Arrange
            const int vendorId = 5;

            var product1 = new Product
            {
                Id = 1,
                VendorId = vendorId,
                Name = "Laptop",
                ReviewCount = 2,
                OverallStars = 10 
            };

            var product2 = new Product
            {
                Id = 2,
                VendorId = vendorId,
                Name = "Mouse",
                ReviewCount = 0,
                OverallStars = 0
            };

            var order = new Order { Id = 100, CreatedAt = DateTime.UtcNow };

            var orderItem1 = new OrderItem
            {
                Id = 1,
                OrderId = 100,
                Order = order,
                ProductId = 1,
                Product = product1,
                Quantity = 2,
                PriceAtPurchase = 1000
            };

            var orderItem2 = new OrderItem
            {
                Id = 2,
                OrderId = 100,
                Order = order,
                ProductId = 2,
                Product = product2,
                Quantity = 3,
                PriceAtPurchase = 50
            };

            product1.OrderItems = new List<OrderItem> { orderItem1 };
            product2.OrderItems = new List<OrderItem> { orderItem2 };

            _productRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(new List<Product> { product1, product2 }.BuildMock());

            _orderItemRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<OrderItem, bool>>>()))
                .Returns(new List<OrderItem> { orderItem1, orderItem2 }.BuildMock());

            _cacheServiceMock
                .Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<VendorStatisticsDto>>>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .Returns<string, Func<Task<VendorStatisticsDto>>, TimeSpan?, CancellationToken>((k, factory, exp, ct) => factory());

            var sut = CreateSut();

            // Act
            var result = await sut.GetVendorStatisticsAsync(vendorId, CancellationToken.None);

            // Assert
            var data = result.ShouldBeSucceeded();
            data.TotalSales.Should().Be(5); // 2 + 3
            data.TotalRevenue.Should().Be(2150); // (2 * 1000) + (3 * 50) = 2150
            data.TotalOrders.Should().Be(1);
            data.TotalProducts.Should().Be(2);
            data.MonthlySales.Should().HaveCount(12);
        }
    }
}
