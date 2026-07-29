using Microsoft.Extensions.Options;
using VendorHub.Settings;
using Path = System.IO.Path;

namespace VendorHub.Services.Storage
{
    public class ImageValidator : IImageValidator
    {
        private readonly ImageStorageOptions _options;
        public ImageValidator(IOptions<ImageStorageOptions> options)
        {
            _options = options.Value;
        }

        public Task<ImageValidationResult> ValidateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Task.FromResult(new ImageValidationResult { ErrorMessage = "No file uploaded" });
            }

            if (file.Length > _options.MaxFileSizeBytes)
                return Task.FromResult(new ImageValidationResult
                { IsValid = false, ErrorMessage = $"File too large (max {_options.MaxFileSizeBytes / 1024 / 1024}MB)." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
                return Task.FromResult(new ImageValidationResult
                { ErrorMessage = $"Invalid type. Allowed: {string.Join(", ", _options.AllowedExtensions)}" });

            if (!_options.AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return Task.FromResult(new ImageValidationResult
                { ErrorMessage = $"Invalid content type: {file.ContentType}" });

            if (!HasValidImageSignature(file))
                return Task.FromResult(new ImageValidationResult
                { IsValid = false, ErrorMessage = "File content doesn't match declared image type." });

            return Task.FromResult(new ImageValidationResult { IsValid = true });
        }

        private bool HasValidImageSignature(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var sig = new byte[8];
            stream.ReadExactly(sig);
            stream.Position = 0;

            if (sig[0] == 0xFF && sig[1] == 0xD8 && sig[2] == 0xFF) return true;
            if (sig[0] == 0x89 && sig[1] == 0x50 && sig[2] == 0x4E && sig[3] == 0x47) return true;
            if (sig[0] == 0x47 && sig[1] == 0x49 && sig[2] == 0x46) return true;
            if (sig[0] == 0x52 && sig[1] == 0x49 && sig[2] == 0x46 && sig[3] == 0x46)
                return true;

            return false;
        }
    }
}
