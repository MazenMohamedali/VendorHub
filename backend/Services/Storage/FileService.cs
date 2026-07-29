using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Security;
using VendorHub.Models;
using VendorHub.Settings;
using Path = System.IO.Path;

namespace VendorHub.Services.Storage
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IImageValidator _validator;
        private readonly ILogger<FileService> _logger;
        private readonly ImageStorageOptions _options;

        public FileService(
            IWebHostEnvironment env,
            IImageValidator validator,
            ILogger<FileService> logger,
            IOptions<ImageStorageOptions> options)
        {
            _env = env;
            _validator = validator;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<string?> SaveImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            ArgumentNullException.ThrowIfNull(file);

            if (string.IsNullOrWhiteSpace(folderName))
                throw new ArgumentException("Folder name required.", nameof(folderName));

            ValidatePathComponent(folderName);

            var validation = await _validator.ValidateAsync(file);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            try
            {
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"{Guid.NewGuid():N}{ext}";

                string targetFolder = Path.Combine(_env.WebRootPath, "Images", folderName);
                var filePath = Path.Combine(targetFolder, fileName);

                var fullPath = Path.GetFullPath(filePath);
                var allowedBase = Path.GetFullPath(Path.Combine(_env.WebRootPath, "Images"));

                if (!fullPath.StartsWith(allowedBase, StringComparison.OrdinalIgnoreCase))
                    throw new SecurityException("Invalid path. Possible directory traversal.");

                Directory.CreateDirectory(targetFolder);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                _logger.LogInformation("Image saved: {Path}", fullPath);
                return fileName;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to save image");
                throw new InvalidOperationException("Failed to save image.", ex);
            }
        }

        public async Task<string?> ReplaceImageAsync(string? oldFileName, IFormFile? newFile, string folderName)
        {
            if (newFile == null || newFile.Length == 0)
                return null;

            if (!string.IsNullOrEmpty(oldFileName))
                await DeleteImageAsync(oldFileName, folderName);

            return await SaveImageAsync(newFile, folderName);
        }

        public Task<bool> DeleteImageAsync(string folderName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Task.FromResult(false);

            ValidatePathComponent(folderName);
            ValidatePathComponent(fileName);

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, "Images", folderName, fileName);
                var fullPath = Path.GetFullPath(filePath);
                var allowedBase = Path.GetFullPath(Path.Combine(_env.WebRootPath, "Images"));

                if (!fullPath.StartsWith(allowedBase, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Traversal attempt: {Path}", filePath);
                    return Task.FromResult(false);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted: {Path}", fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {File}", fileName);
                return Task.FromResult(false);
            }
        }

        public bool ImageExists(string folderName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            var path = Path.Combine(_env.WebRootPath, "Images", folderName, fileName);
            return File.Exists(path);
        }

        private static void ValidatePathComponent(string name)
        {
            if (name.Contains("..") || name.Contains('/') || name.Contains('\\'))
                throw new ArgumentException($"Invalid path component: {name}");
        }

        public string BuildImageUrl(string folderName, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;

            if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
                return fileName;

            var baseUrl = _options.BaseUrl?.TrimEnd('/') ?? string.Empty;
            var cleanFileName = fileName.TrimStart('/');

            if (cleanFileName.StartsWith("Images/", StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrEmpty(baseUrl) ? $"/{cleanFileName}" : $"{baseUrl}/{cleanFileName}";

            return string.IsNullOrEmpty(baseUrl) 
                ? $"/Images/{folderName}/{cleanFileName}" 
                : $"{baseUrl}/Images/{folderName}/{cleanFileName}";
        }
    }
}
