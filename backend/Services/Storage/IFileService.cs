
namespace VendorHub.Services.Storage
{
    public interface IFileService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folderName);
        Task<string?> ReplaceImageAsync(string? oldFileName, IFormFile? newFile, string folderName);
        Task<bool> DeleteImageAsync(string folderName, string fileName);
        bool ImageExists(string folderName, string fileName);
        string BuildImageUrl(string folderName, string? fileName);

    }
}
