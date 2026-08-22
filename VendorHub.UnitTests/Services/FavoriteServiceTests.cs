using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VendorHub.DTOs.Favorite;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class FavoriteServiceTests
    {
        private readonly Mock<IGeneralRepository<Favorite>> _favoriteRepositoryMock;
        private readonly Mock<ILogger<FavoriteService>> _loggerMock;

        public FavoriteServiceTests()
        {
            _favoriteRepositoryMock = new Mock<IGeneralRepository<Favorite>>();
            _loggerMock = new Mock<ILogger<FavoriteService>>();
        }

        private FavoriteService CreateSut()
        {
            return new FavoriteService(_favoriteRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddToFavoritesAsync_WhenValid_AddsFavoriteAndReturnsCreated()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;
            var sut = CreateSut();

            // Act
            var result = await sut.AddToFavoritesAsync(productId, customerId, CancellationToken.None);

            // Assert
            result.ShouldBeCreated();
            _favoriteRepositoryMock.Verify(r => r.AddAsync(It.Is<Favorite>(f => f.ProductId == productId && f.CustomerId == customerId), It.IsAny<CancellationToken>()), Times.Once);
            _favoriteRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddToFavoritesAsync_WhenDuplicateFavoriteThrowsDbUpdateException_ReturnsInvalidInput()
        {
            // Arrange
            const int productId = 10;
            const int customerId = 1;

            _favoriteRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Duplicate unique index violation", new Exception()));

            var sut = CreateSut();

            // Act
            var result = await sut.AddToFavoritesAsync(productId, customerId, CancellationToken.None);

            // Assert
            result.ShouldBeInvalidInput();
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_WhenFavoriteDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const int productId = 5;
            const int customerId = 1;

            _favoriteRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Favorite, bool>>>()))
                .Returns(new List<Favorite>().BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.RemoveFromFavoritesAsync(productId, customerId, CancellationToken.None);

            // Assert
            result.ShouldBeNotFound();
            _favoriteRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_WhenFavoriteExists_DeletesAndReturnsSucceeded()
        {
            // Arrange
            const int productId = 5;
            const int customerId = 1;
            var existingFavorite = new Favorite { ProductId = productId, CustomerId = customerId };

            _favoriteRepositoryMock
                .Setup(r => r.GetBy(It.IsAny<Expression<Func<Favorite, bool>>>()))
                .Returns(new List<Favorite> { existingFavorite }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.RemoveFromFavoritesAsync(productId, customerId, CancellationToken.None);

            // Assert
            result.ShouldBeSucceeded();
            _favoriteRepositoryMock.Verify(r => r.Delete(existingFavorite), Times.Once);
            _favoriteRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCustomerFavoritesAsync_WhenInvoked_ReturnsMappedFavoriteDtosWithAverageRating()
        {
            // Arrange
            const int customerId = 1;
            var product1 = new Product
            {
                Id = 10,
                Name = "Headphones",
                Price = 150,
                ImgUrl = "headphones.png",
                ReviewCount = 4,
                OverallStars = 18 
            };

            var product2 = new Product
            {
                Id = 20,
                Name = "Keyboard",
                Price = 80,
                ImgUrl = "keyboard.png",
                ReviewCount = 0,
                OverallStars = 0 
            };

            var favorites = new List<Favorite>
            {
                new() { CustomerId = customerId, ProductId = 10, Product = product1, AddedAt = DateTime.UtcNow },
                new() { CustomerId = customerId, ProductId = 20, Product = product2, AddedAt = DateTime.UtcNow }
            };

            _favoriteRepositoryMock
                .Setup(r => r.GetByAsNoTracking(It.IsAny<Expression<Func<Favorite, bool>>>()))
                .Returns(favorites.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetCustomerFavoritesAsync(customerId, CancellationToken.None);

            // Assert
            var list = result.ShouldBeSucceeded().ToList();
            list.Should().HaveCount(2);

            list[0].Id.Should().Be(10);
            list[0].AverageStars.Should().Be(4.5);

            list[1].Id.Should().Be(20);
            list[1].AverageStars.Should().Be(0);
        }
    }
}
