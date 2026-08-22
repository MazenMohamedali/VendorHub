
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorHub.Constants;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.Services.Storage;
using VendorHub.UnitTests.Extensions;
using VendorHub.UnitTests.TestHelpers;
namespace VendorHub.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IGeneralRepository<Product>> _productRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IGeneralRepository<Product>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _fileServiceMock = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<ProductService>>();
        }

        private ProductService CreateSut()
        {
            return new ProductService(
                _productRepositoryMock.Object,
                _cacheServiceMock.Object,
                _fileServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_WithoutImage_CreatesProductAndSavesToDatabase()
        {
            // Arrange
            var dto = new AddProductDto
            {
                Name = "Mechanical Keyboard",
                Price = 120,
                Quantity = 15,
                CategoryId = 2,
                ImageFile = null
            };
            var sut = CreateSut();

            // Act
            var result = await sut.AddAsync(dto);

            // Assert
            var data = result.ShouldBeCreated();
            data.Name.Should().Be("Mechanical Keyboard");

            _productRepositoryMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == dto.Name && p.ImgUrl == string.Empty && p.Status == ProductStatus.PENDING), It.IsAny<CancellationToken>()), Times.Once);
            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _fileServiceMock.Verify(f => f.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_WithImage_SavesImageAndStoresPath()
        {
            // Arrange
            var image = TestHelper.CreateDummyFile();
            var dto = new AddProductDto
            {
                Name = "Gaming Mouse",
                Price = 60,
                Quantity = 30,
                CategoryId = 2,
                ImageFile = image
            };
            const string uploadedImagePath = "products/mouse.png";

            _fileServiceMock
                .Setup(f => f.SaveImageAsync(image, ImageFolders.Products))
                .ReturnsAsync(uploadedImagePath);

            var sut = CreateSut();

            // Act
            var result = await sut.AddAsync(dto);

            // Assert
            result.ShouldBeCreated();
            _fileServiceMock.Verify(f => f.SaveImageAsync(image, ImageFolders.Products), Times.Once);
            _productRepositoryMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.ImgUrl == uploadedImagePath && p.Status == ProductStatus.PENDING), It.IsAny<CancellationToken>()), Times.Once);
            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(new EditProductDto { Name = "Updated Product" }, 1);

            // Assert
            result.ShouldBeNotFound();
            _productRepositoryMock.VerifyNoDatabaseMutations();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task UpdateAsync_WhenValidWithNewImage_ReplacesImageAndEvictsCaches()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 10,
                Name = "Old Monitor",
                ImgUrl = "products/old-screen.png",
                Price = 200
            };
            var file = TestHelper.CreateDummyFile("new-screen.png");
            var dto = new EditProductDto
            {
                Name = "UltraWide Monitor",
                Price = 350,
                ImageFile = file
            };

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _fileServiceMock
                .Setup(f => f.ReplaceImageAsync("products/old-screen.png", file, ImageFolders.Products))
                .ReturnsAsync("products/new-screen.png");

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(dto, 10);

            // Assert
            result.ShouldBeSucceeded();
            existingProduct.ImgUrl.Should().Be("products/new-screen.png");

            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.TOP_PRODUCTS);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.ProductDetails(10));
        }

        [Fact]
        public async Task UpdateAsync_WhenDbUpdateConcurrencyOccurs_ReturnsErrorResult()
        {
            // Arrange
            var product = new Product { Id = 4, Name = "Smartphone" };
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(new EditProductDto { Name = "Smartphone Pro" }, 4);

            // Assert
            result.ShouldBeError();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductExists_SetsStatusToArchivedAndEvictsCaches()
        {
            // Arrange
            var product = new Product { Id = 7, Status = ProductStatus.REVIEWED };
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteProductAsync(7);

            // Assert
            result.ShouldBeSucceeded();
            product.Status.Should().Be(ProductStatus.Archived);

            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.TOP_PRODUCTS);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.ProductDetails(7));
        }

        [Fact]
        public async Task ApproveProductAsync_WhenProductExists_SetsStatusToReviewedAndEvictsCaches()
        {
            // Arrange
            var product = new Product { Id = 8, Status = ProductStatus.PENDING };
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var sut = CreateSut();

            // Act
            var result = await sut.ApproveProductAsync(8);

            // Assert
            result.ShouldBeSucceeded();
            product.Status.Should().Be(ProductStatus.REVIEWED);

            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.TOP_PRODUCTS);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.ProductDetails(8));
        }

        [Fact]
        public async Task RejectProductAsync_WhenProductExists_SetsStatusToRejectedAndEvictsCaches()
        {
            // Arrange
            var product = new Product { Id = 9, Status = ProductStatus.PENDING };
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var sut = CreateSut();

            // Act
            var result = await sut.RejectProductAsync(9);

            // Assert
            result.ShouldBeSucceeded();
            product.Status.Should().Be(ProductStatus.REJECTED);

            _productRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.TOP_PRODUCTS);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.ProductDetails(9));
        }

        [Theory]
        [InlineData(1)] // delete 
        [InlineData(2)] // approve
        [InlineData(3)] // reject
        public async Task StatusOperations_WhenProductNotFound_ReturnsNotFound(int operationType)
        {
            // Arrange
            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var sut = CreateSut();

            // Act
            GeneralResponse result = operationType switch
            {
                1 => await sut.DeleteProductAsync(404),
                2 => await sut.ApproveProductAsync(404),
                3 => await sut.RejectProductAsync(404),
                _ => throw new ArgumentOutOfRangeException(nameof(operationType))
            };

            // Assert
            result.ShouldBeNotFound();
            _productRepositoryMock.VerifyNoDatabaseMutations();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }
    }
}
