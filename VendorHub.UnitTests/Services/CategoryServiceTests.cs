using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using VendorHub.Constants;
using VendorHub.DTOs.CategoryDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.Services.Storage;
using VendorHub.UnitTests.Extensions;
using VendorHub.UnitTests.TestHelpers;

namespace VendorHub.UnitTests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<IGeneralRepository<Category>> _categoryRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<IGeneralRepository<Category>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _fileServiceMock = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<CategoryService>>();
        }

        private CategoryService CreateSut()
        {
            return new CategoryService(
                _categoryRepositoryMock.Object,
                _cacheServiceMock.Object,
                _fileServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchByNameAsync_WhenTermIsInvalid_ReturnInvalidInput(string? invalidTerm)
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.SearchByNameAsync(invalidTerm!);

            // Assert
            result.ShouldBeInvalidInput();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task AddAsync_WhenNameIsMissingOrWhitespace_ReturnInvalidInput(string? invalidName)
        {
            // Arrange
            var dto = new CreateCategoryDto { NormalizedName = invalidName! };
            var sut = CreateSut();

            // Act
            var result = await sut.AddAsync(dto);

            // Assert
            result.ShouldBeInvalidInput();
            _categoryRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task AddAsync_ValidCategoryWithoutImage_AddsAndInvalidateCache()
        {
            // Arrange
            var dto = new CreateCategoryDto { NormalizedName = "Electronics", ImageFile = null };
            var sut = CreateSut();

            // Act
            var result = await sut.AddAsync(dto);

            // Assert
            var createdCategory = result.ShouldBeCreated();
            createdCategory.Name.Should().Be("electronics");

            _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "electronics" && c.ImageUrl == null), It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ALL_CATEGORIES, It.IsAny<CancellationToken>()), Times.Once);
            _fileServiceMock.Verify(f => f.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_ValidCategoryWithImage_SavesImageAndStoresPath()
        {
            // Arrange
            var image = TestHelper.CreateDummyFile("food.png");
            var dto = new CreateCategoryDto { NormalizedName = "food", ImageFile = image };

            _fileServiceMock
                .Setup(f => f.SaveImageAsync(image, ImageFolders.Categories))
                .ReturnsAsync("categories/food.png");
            var sut = CreateSut();

            // Act
            var result = await sut.AddAsync(dto);

            // Assert
            result.ShouldBeCreated();
            _fileServiceMock.Verify(f => f.SaveImageAsync(image, ImageFolders.Categories), Times.Once);
            _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.ImageUrl == "categories/food.png"), It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ALL_CATEGORIES, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrowsException_RollsBackSavedImageAndRethrows()
        {
            // Arrange
            var image = TestHelper.CreateDummyFile("book.png");
            var dto = new CreateCategoryDto { NormalizedName = "books", ImageFile = image };
            const string savedImagePath = "categories/book.png";

            _fileServiceMock
                .Setup(f => f.SaveImageAsync(image, ImageFolders.Categories))
                .ReturnsAsync(savedImagePath);

            _categoryRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB connection error"));

            var sut = CreateSut();

            // Act
            var act = () => sut.AddAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _fileServiceMock.Verify(f => f.DeleteImageAsync(ImageFolders.Categories, savedImagePath), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryNotFound_ReturnsNotFound()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, new UpdateCategoryDto { NormalizedName = "Updated" });

            // Assert
            result.ShouldBeNotFound();
            _categoryRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateAsync_WhenNewNameAlreadyExistsInAnotherCategory_ReturnsInvalidInput()
        {
            // Arrange
            var existingCategory = new Category { Id = 1, Name = "OriginalName" };
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCategory);

            _categoryRepositoryMock
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, new UpdateCategoryDto { NormalizedName = "DuplicateName" });

            // Assert
            result.ShouldBeInvalidInput();
            _categoryRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task UpdateAsync_ValidUpdateWithNewImage_replacesImageAndClearsCaches()
        {
            // Arrange
            var image = TestHelper.CreateDummyFile("new.png");
            var category = new Category { Id = 1, Name = "OldName", ImageUrl = "categories/old.png", IsActive = true };
            var dto = new UpdateCategoryDto
            {
                NormalizedName = "NewName",
                IsActive = false,
                ImageFile = image
            };

            const string newImageUrl = "categories/new.png";

            _categoryRepositoryMock
                .Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(e => e.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _fileServiceMock
                .Setup(f => f.ReplaceImageAsync(category.ImageUrl, image, ImageFolders.Categories))
                .ReturnsAsync(newImageUrl);

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, dto);

            // Assert
            result.ShouldBeSucceeded();
            category.Name.Should().Be("newname");
            category.ImageUrl.Should().Be(newImageUrl);
            category.IsActive.Should().BeFalse();

            _categoryRepositoryMock.Verify(r => r.Update(category), Times.Once);
            _categoryRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.ALL_CATEGORIES);
            _cacheServiceMock.VerifyCacheEvicted(CacheKeys.CategoryDetails(1));
        }

        [Fact]
        public async Task UpdateAsync_WhenDbUpdateConcurrencyOccurs_ReturnErrorResult()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "food", IsActive = true };

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.UpdateAsync(1, new UpdateCategoryDto { IsActive = false });

            // Assert
            result.ShouldBeError();
            _cacheServiceMock.VerifyCacheNotEvicted();
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteAsync(99);

            // Assert
            result.ShouldBeNotFound();
            _categoryRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task HardDeleteAsync_WhenCategoryNotFound_ReturnsNotFound()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.HardDeleteAsync(404);

            // Assert
            result.ShouldBeNotFound();
            _categoryRepositoryMock.VerifyNoDatabaseMutations();
            _fileServiceMock.Verify(f => f.DeleteImageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

    }
}