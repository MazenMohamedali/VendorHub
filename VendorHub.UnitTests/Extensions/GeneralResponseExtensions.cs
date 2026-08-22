using FluentAssertions;
using Moq;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;

namespace VendorHub.UnitTests.Extensions
{
    public static class GeneralResponseExtensions
    {
        public static void ShouldBeInvalidInput<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.InvalidInput);
            response.Data.Should().BeNull();
        }

        public static void ShouldBeUnauthenticated<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.Unauthenticated);
            response.Data.Should().BeNull();
        }

        public static void ShouldBeNotFound<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.NotFound);
            response.Data.Should().BeNull();
        }

        public static void ShouldBeNotFound(this GeneralResponse response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.NotFound);
        }

        public static void ShouldBeForbidden<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.Forbidden);
            response.Data.Should().BeNull();
        }

        public static T ShouldBeSucceeded<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeTrue();
            response.Status.Should().Be(ResultStatus.Success);
            return response.Data!;
        }

        public static T ShouldBeCreated<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeTrue();
            response.Status.Should().Be(ResultStatus.Created);
            return response.Data!;
        }

        public static void ShouldBeCreated(this GeneralResponse response)
        {
            response.Success.Should().BeTrue();
            response.Status.Should().Be(ResultStatus.Created);
        }

        public static void ShouldBeInvalidInput(this GeneralResponse response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.InvalidInput);
        }

        public static void ShouldBeForbidden(this GeneralResponse response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.Forbidden);
        }

        public static void ShouldBeSucceeded(this GeneralResponse response)
        {
            response.Success.Should().BeTrue();
            response.Status.Should().Be(ResultStatus.Success);
        }

        public static void ShouldBeError(this GeneralResponse response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.Error);
        }

        public static void ShouldBeError<T>(this GeneralResponse<T> response)
        {
            response.Success.Should().BeFalse();
            response.Status.Should().Be(ResultStatus.Error);
            response.Data.Should().BeNull();
        }

        public static void VerifyNoDatabaseMutations<T>(this Mock<IGeneralRepository<T>> repoMock) where T : class
        {
            repoMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
            repoMock.Verify(r => r.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()), Times.Never);
            repoMock.Verify(r => r.Update(It.IsAny<T>()), Times.Never);
            repoMock.Verify(r => r.Delete(It.IsAny<T>()), Times.Never);
        }


        public static void VerifyCacheEvicted(this Mock<ICacheService> cacheServiceMock, string key)
        {
            cacheServiceMock.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        public static void VerifyCacheNotEvicted(this Mock<ICacheService> cacheServiceMock, string? key = null)
        {
            if (key == null)
                cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            else
                cacheServiceMock.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
