using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using VendorHub.DTOs.ReviewDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IGeneralRepository<Review>> _reviewRepositoryMock;
        private readonly Mock<IGeneralRepository<Product>> _productRepositoryMock;
        private readonly Mock<IGeneralRepository<Order>> _orderRepositoryMock;
        private readonly Mock<ILogger<ReviewService>> _loggerMock;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IGeneralRepository<Review>>();
            _productRepositoryMock = new Mock<IGeneralRepository<Product>>();
            _orderRepositoryMock = new Mock<IGeneralRepository<Order>>();
            _loggerMock = new Mock<ILogger<ReviewService>>();
        }

        private ReviewService CreateSut()
        {
            return new ReviewService(
                _reviewRepositoryMock.Object,
                _productRepositoryMock.Object,
                _orderRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AddReviewAsync_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            const int productId = 99;
            const int customerId = 1;
            var dto = new CreateReviewDto { Rating = 5, Comment = "Great!" };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.AddReviewAsync(productId, customerId, dto);

            // Assert
            result.ShouldBeNotFound();
            _reviewRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task AddReviewAsync_WhenCustomerDidNotOrderProduct_ReturnsForbidden()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;
            var product = new Product { Id = productId, Name = "Smartphone" };
            var dto = new CreateReviewDto { Rating = 4, Comment = "Good" };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // No orders containing this product for the customer
            _orderRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Order>().BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.AddReviewAsync(productId, customerId, dto);

            // Assert
            result.ShouldBeForbidden();
            _reviewRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task AddReviewAsync_WhenCustomerAlreadyReviewedProduct_ReturnsInvalidInput()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;
            var product = new Product { Id = productId, Name = "Smartphone" };
            var dto = new CreateReviewDto { Rating = 5, Comment = "Amazing!" };

            var order = new Order
            {
                Id = 100,
                CustomerId = customerId,
                Items = new List<OrderItem>
                {
                    new() { ProductId = productId, Quantity = 1 }
                }
            };

            var existingReview = new Review
            {
                Id = 50,
                ProductId = productId,
                CustomerId = customerId,
                Rating = 4
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Order> { order }.BuildMock());

            _reviewRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Review> { existingReview }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.AddReviewAsync(productId, customerId, dto);

            // Assert
            result.ShouldBeInvalidInput();
            _reviewRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task AddReviewAsync_WhenValid_AddsReviewIncrementsRatingAndReturnsCreated()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Smartphone",
                ReviewCount = 2,
                OverallStars = 8 // Average 4.0
            };

            var dto = new CreateReviewDto { Rating = 5, Comment = "Best phone ever!" };

            var order = new Order
            {
                Id = 100,
                CustomerId = customerId,
                Items = new List<OrderItem>
                {
                    new() { ProductId = productId, Quantity = 1 }
                }
            };

            var customer = new Customer { Id = customerId, FirstName = "John", SecondName = "Doe" };
            var persistedReview = new Review
            {
                Id = 1,
                ProductId = productId,
                CustomerId = customerId,
                Customer = customer,
                Rating = 5,
                Comment = "Best phone ever!",
                CreatedAt = DateTime.UtcNow
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Order> { order }.BuildMock());

            _reviewRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Review>().BuildMock()); // No prior reviews

            _reviewRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Review, bool>>>()))
                .Returns(new List<Review> { persistedReview }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.AddReviewAsync(productId, customerId, dto);

            // Assert
            var data = result.ShouldBeCreated();
            data.Rating.Should().Be(5);
            data.Comment.Should().Be("Best phone ever!");
            data.CustomerName.Should().Be("John Doe");

            product.ReviewCount.Should().Be(3);
            product.OverallStars.Should().Be(13); // 8 + 5 = 13

            _reviewRepositoryMock.Verify(r => r.AddAsync(It.Is<Review>(rv => rv.ProductId == productId && rv.CustomerId == customerId && rv.Rating == 5), It.IsAny<CancellationToken>()), Times.Once);
            _reviewRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddReviewAsync_WhenDbUpdateConcurrencyOccurs_ReturnsError()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;
            var product = new Product { Id = productId, Name = "Smartphone", ReviewCount = 1, OverallStars = 4 };
            var dto = new CreateReviewDto { Rating = 5, Comment = "Great!" };

            var order = new Order
            {
                Id = 100,
                CustomerId = customerId,
                Items = new List<OrderItem> { new() { ProductId = productId, Quantity = 1 } }
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Order> { order }.BuildMock());

            _reviewRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(new List<Review>().BuildMock());

            _reviewRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.AddReviewAsync(productId, customerId, dto);

            // Assert
            result.ShouldBeError();
        }

        [Fact]
        public async Task GetProductReviewsAsync_WhenInvoked_ReturnsPagedReviews()
        {
            // Arrange
            const int productId = 5;
            var customer = new Customer { Id = 1, FirstName = "Alice", SecondName = "Smith" };
            var reviews = new List<Review>
            {
                new() { Id = 1, ProductId = productId, CustomerId = 1, Customer = customer, Rating = 5, Comment = "Superb", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Id = 2, ProductId = productId, CustomerId = 2, Customer = null, Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow },
                new() { Id = 3, ProductId = 99, CustomerId = 3, Rating = 1, Comment = "Different product", CreatedAt = DateTime.UtcNow }
            };

            _reviewRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(reviews.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetProductReviewsAsync(productId, page: 1, pageSize: 10);

            // Assert
            var pagedData = result.ShouldBeSucceeded();
            pagedData.TotalCount.Should().Be(2);
            pagedData.Items.Should().NotBeNull();
            pagedData.Items!.Should().HaveCount(2);
            pagedData.Items.First().Rating.Should().Be(4); // Sorted descending by CreatedAt
            pagedData.Items.Last().CustomerName.Should().Be("Alice Smith");
        }
    }
}
