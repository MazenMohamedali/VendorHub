using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using VendorHub.DTOs.OrderDto;
using VendorHub.Events;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IGeneralRepository<Order>> _orderRepositoryMock;
        private readonly Mock<IGeneralRepository<OrderItem>> _orderItemRepositoryMock;
        private readonly Mock<IGeneralRepository<Product>> _productRepositoryMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IEventQueue<OrderPlacedEvent>> _orderPlacedEventQueueMock;
        private readonly Mock<IEventQueue<OrderStatusChangedEvent>> _statusChangedEventQueueMock;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IGeneralRepository<Order>>();
            _orderItemRepositoryMock = new Mock<IGeneralRepository<OrderItem>>();
            _productRepositoryMock = new Mock<IGeneralRepository<Product>>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _orderPlacedEventQueueMock = new Mock<IEventQueue<OrderPlacedEvent>>();
            _statusChangedEventQueueMock = new Mock<IEventQueue<OrderStatusChangedEvent>>();
        }

        private OrderService CreateSut()
        {
            return new OrderService(
                _orderRepositoryMock.Object,
                _orderItemRepositoryMock.Object,
                _productRepositoryMock.Object,
                _loggerMock.Object,
                _orderPlacedEventQueueMock.Object,
                _statusChangedEventQueueMock.Object
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WhenValidCheckout_DeductsStockSaveOrderAndEnqueuesEvent()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                DeliveryAddress = "cairo",
                PhoneNumber = "01008429511",
                CartItems = new List<CartItemDto>
                {
                    new() { ProductId = 1, Quantity = 2 },
                    new() { ProductId = 2, Quantity = 1 }
                }
            };

            var product1 = new Product { Id = 1, Name = "Item A", Price = 100, Quantity = 10, Status = ProductStatus.REVIEWED, VendorId = 201 };
            var product2 = new Product { Id = 2, Name = "Item B", Price = 50, Quantity = 5, Status = ProductStatus.REVIEWED, VendorId = 202 };

            var transactionMock = new Mock<IDbContextTransaction>();
            _orderItemRepositoryMock
                .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            var productsMock = new List<Product> { product1, product2 }.BuildMock();

            _productRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(productsMock);

            var sut = CreateSut();

            // Act 
            var result = await sut.CreateOrderAsync(dto, 42, CancellationToken.None);

            // Assert 
            var data = result.ShouldBeCreated();
            data.TotalPrice.Should().Be(250); 

            product1.Quantity.Should().Be(8);
            product2.Quantity.Should().Be(4);

            _orderRepositoryMock.Verify(r => r.AddAsync(It.Is<Order>(o => o.CustomerId == 42 && o.TotalPrice == 250 && o.TotalItemsCount == 3), It.IsAny<CancellationToken>()), Times.Once);
            _orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            _orderPlacedEventQueueMock.Verify(q => q.EnqueueAsync(
                It.Is<OrderPlacedEvent>(e => e.Order.CustomerId == 42 && e.VendorSummaries.Count == 2),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartIsEmpty_ReturnsInvalidInput()
        {
            // Arrange
            var dto = new CreateOrderDto { CartItems = new List<CartItemDto>() };
            var sut = CreateSut();

            // Act
            var result = await sut.CreateOrderAsync(dto, 1, CancellationToken.None);

            // Assert
            result.ShouldBeInvalidInput();
            _orderItemRepositoryMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _orderRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProductNotFound_ReturnsInvalidInput()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                CartItems = new List<CartItemDto> { new() { ProductId = 10, Quantity = 2 } }
            };

            var transactionMock = new Mock<IDbContextTransaction>();
            _orderItemRepositoryMock
                .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            var emptyProductsMock = new List<Product>().BuildMock();

            _productRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(emptyProductsMock);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateOrderAsync(dto, 1, CancellationToken.None);

            // Assert
            result.ShouldBeInvalidInput();
            _orderRepositoryMock.VerifyNoDatabaseMutations();
            _orderPlacedEventQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProductIsNotReviewed_ReturnsInvalidInput()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                CartItems = new List<CartItemDto> { new() { ProductId = 5, Quantity = 1 } }
            };

            var unreviewedProduct = new Product
            {
                Id = 5,
                Name = "Draft Product",
                Status = ProductStatus.PENDING,
                Quantity = 10,
                Price = 100
            };

            var transactionMock = new Mock<IDbContextTransaction>();
            _orderItemRepositoryMock
                .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            var unreviewedMock = new List<Product> { unreviewedProduct }.BuildMock();

            _productRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(unreviewedMock);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateOrderAsync(dto, 1, CancellationToken.None);

            // Assert
            result.ShouldBeInvalidInput();
            _orderRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task CreateOrderAsync_WhenStockIsInsufficient_ReturnsInvalidInput()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                CartItems = new List<CartItemDto> { new() { ProductId = 5, Quantity = 10 } }
            };

            var product = new Product
            {
                Id = 5,
                Name = "Low Stock Item",
                Status = ProductStatus.REVIEWED,
                Quantity = 3,
                Price = 50
            };

            var transactionMock = new Mock<IDbContextTransaction>();
            _orderItemRepositoryMock
                .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            var lowStockMock = new List<Product> { product }.BuildMock();

            _productRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(lowStockMock);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateOrderAsync(dto, 1, CancellationToken.None);

            // Assert
            result.ShouldBeInvalidInput();
            product.Quantity.Should().Be(3);
            _orderRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var emptyItemsMock = new List<OrderItem>().BuildMock();

            _orderItemRepositoryMock
                .Setup(r => r.GetAll())
                .Returns(emptyItemsMock);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateOrderStatusAsync(99, vendorId: 10, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

            // Assert
            result.ShouldBeNotFound();
            _orderRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenVendorHasNoItemsInOrder_ReturnsForbidden()
        {
            // Arrange
            var existingOrder = new Order { Id = 1, CustomerId = 5 };
            var emptyItemsMock = new List<OrderItem>().BuildMock();

            _orderItemRepositoryMock
                .Setup(r => r.GetAll())
                .Returns(emptyItemsMock);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingOrder);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateOrderStatusAsync(1, vendorId: 10, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

            // Assert
            result.ShouldBeForbidden();
            _orderRepositoryMock.VerifyNoDatabaseMutations();
            _statusChangedEventQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenAllItemsDelivered_UpdatesGlobalStatusToDeliveredAndEnqueuesEvent()
        {
            // Arrange
            const int orderId = 100;
            const int vendorId = 20;

            var product = new Product { Id = 1, VendorId = vendorId, Name = "Laptop" };
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = 1,
                Product = product,
                Quantity = 2,
                ItemStatus = OrderStatus.Pending
            };

            var order = new Order
            {
                Id = orderId,
                CustomerId = 7,
                TotalItemsCount = 2,
                Status = OrderStatus.Pending,
                DeliveredItemsCount = 0
            };

            var itemsMock = new List<OrderItem> { orderItem }.BuildMock();

            _orderItemRepositoryMock
                .Setup(r => r.GetAll())
                .Returns(itemsMock);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateOrderStatusAsync(orderId, vendorId, new UpdateOrderStatusDto { Status = OrderStatus.Delivered });

            // Assert
            result.ShouldBeSucceeded();
            orderItem.ItemStatus.Should().Be(OrderStatus.Delivered);
            order.DeliveredItemsCount.Should().Be(2);
            order.Status.Should().Be(OrderStatus.Delivered);

            _orderRepositoryMock.Verify(r => r.Update(order), Times.Once);
            _orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);

            _statusChangedEventQueueMock.Verify(q => q.EnqueueAsync(
                It.Is<OrderStatusChangedEvent>(e => e.CustomerId == 7 && e.OrderId == orderId && e.NewStatus == nameof(OrderStatus.Delivered)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAvailableStatusesAsync_ReturnsAllEnumNames()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetAvailableStatusesAsync();

            // Assert
            result.ShouldBeSucceeded();
            result.Data.Should().NotBeNull();
            result.Data!.Select(s => s.Value).Should().BeEquivalentTo(Enum.GetNames(typeof(OrderStatus)));
        }
    }
}
