using Microsoft.AspNetCore.Http;
using Moq;

namespace VendorHub.UnitTests.TestHelpers
{
    public static class TestHelper
    {
        public static IFormFile CreateDummyFile(
            string fileName = "test.png",
            string contentType = "image/png",
            byte[]? contentBytes = null,
            long? overrideLength = null)
        {
            var bytes = contentBytes ?? new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(overrideLength ?? bytes.Length);

            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));

            return fileMock.Object;
        }
    }
}
