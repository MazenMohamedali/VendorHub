using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using VendorHub.Services.Storage;
using VendorHub.Settings;
using VendorHub.UnitTests.TestHelpers;

namespace VendorHub.UnitTests.Services.Storage
{
    public class ImageValidatorTests
    {
        private readonly ImageStorageOptions _options;

        public ImageValidatorTests()
        {
            _options = new ImageStorageOptions
            {
                MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
                AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" },
                AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" }
            };
        }

        private ImageValidator CreateSut(ImageStorageOptions? options = null)
        {
            var optionsMock = Options.Create(options ?? _options);
            return new ImageValidator(optionsMock);
        }

        [Fact]
        public async Task ValidateAsync_WhenFileIsNull_ReturnsNoFileUploaded()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.NoFileUploaded);
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ValidateAsync_WhenFileIsEmpty_ReturnsNoFileUploaded()
        {
            // Arrange
            var emptyFile = TestHelper.CreateDummyFile(contentBytes: Array.Empty<byte>(), overrideLength: 0);
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(emptyFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.NoFileUploaded);
        }

        [Fact]
        public async Task ValidateAsync_WhenFileExceedsMaxSize_ReturnsFileTooLarge()
        {
            // Arrange
            var largeFile = TestHelper.CreateDummyFile(overrideLength: 10 * 1024 * 1024); // 10MB
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(largeFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.FileTooLarge);
            result.ErrorMessage.Should().Contain("5MB");
        }

        [Theory]
        [InlineData("malicious.exe")]
        [InlineData("document.pdf")]
        [InlineData("vector.svg")]
        [InlineData("script.js")]
        [InlineData("noextension")]
        public async Task ValidateAsync_WhenExtensionIsNotAllowed_ReturnsInvalidExtension(string fileName)
        {
            // Arrange
            var file = TestHelper.CreateDummyFile(fileName: fileName, contentType: "image/png");
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(file);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.InvalidExtension);
        }

        [Theory]
        [InlineData("application/octet-stream")]
        [InlineData("application/pdf")]
        [InlineData("text/html")]
        [InlineData("video/mp4")]
        public async Task ValidateAsync_WhenContentTypeIsNotAllowed_ReturnsInvalidContentType(string contentType)
        {
            // Arrange
            var file = TestHelper.CreateDummyFile(fileName: "valid.png", contentType: contentType);
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(file);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.InvalidContentType);
        }

        [Fact]
        public async Task ValidateAsync_WhenFileHasFakeExtensionAndInvalidSignature_ReturnsSignatureMismatch()
        {
            // Arrange - Text file disguised with .png extension
            var disguisedFile = TestHelper.CreateDummyFile(
                fileName: "virus.png",
                contentType: "image/png",
                contentBytes: System.Text.Encoding.UTF8.GetBytes("Hello Malicious Content! This is not a real image.")
            );
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(disguisedFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorCode.Should().Be(ImageValidationError.SignatureMismatch);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidPng_ReturnsSuccess()
        {
            // Arrange - PNG Magic Bytes: 89 50 4E 47 0D 0A 1A 0A
            var validPng = TestHelper.CreateDummyFile(
                fileName: "avatar.png",
                contentType: "image/png",
                contentBytes: new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            );
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(validPng);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorCode.Should().Be(ImageValidationError.None);
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task ValidateAsync_WhenValidJpeg_ReturnsSuccess()
        {
            // Arrange - JPEG Magic Bytes: FF D8 FF E0 00 10 4A 46
            var validJpeg = TestHelper.CreateDummyFile(
                fileName: "photo.jpg",
                contentType: "image/jpeg",
                contentBytes: new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 }
            );
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(validJpeg);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorCode.Should().Be(ImageValidationError.None);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidGif_ReturnsSuccess()
        {
            // Arrange - GIF89a Magic Bytes: 47 49 46 38 39 61
            var validGif = TestHelper.CreateDummyFile(
                fileName: "animation.gif",
                contentType: "image/gif",
                contentBytes: new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x00, 0x00 }
            );
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(validGif);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorCode.Should().Be(ImageValidationError.None);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidWebP_ReturnsSuccess()
        {
            // Arrange - WebP / RIFF Magic Bytes: 52 49 46 46 ...
            var validWebp = TestHelper.CreateDummyFile(
                fileName: "banner.webp",
                contentType: "image/webp",
                contentBytes: new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00 }
            );
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(validWebp);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorCode.Should().Be(ImageValidationError.None);
        }
    }
}
