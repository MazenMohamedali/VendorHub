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
                return Task.FromResult(ImageValidationResult.Failure(ImageValidationError.NoFileUploaded, "No file uploaded."));
            }

            if (file.Length > _options.MaxFileSizeBytes)
            {
                return Task.FromResult(ImageValidationResult.Failure(
                    ImageValidationError.FileTooLarge,
                    $"File size exceeds the allowed limit of {_options.MaxFileSizeBytes / 1024 / 1024}MB."));
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
            {
                return Task.FromResult(ImageValidationResult.Failure(
                    ImageValidationError.InvalidExtension,
                    $"Invalid file extension '{extension}'. Allowed extensions: {string.Join(", ", _options.AllowedExtensions)}."));
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) || !_options.AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return Task.FromResult(ImageValidationResult.Failure(
                    ImageValidationError.InvalidContentType,
                    $"Invalid content type '{file.ContentType}'."));
            }

            if (!HasValidImageSignature(file))
            {
                return Task.FromResult(ImageValidationResult.Failure(
                    ImageValidationError.SignatureMismatch,
                    "File header signature does not match valid image formats."));
            }

            return Task.FromResult(ImageValidationResult.Success());
        }

        private static bool HasValidImageSignature(IFormFile file)
        {
            if (file.Length < 4) return false;

            try
            {
                using var stream = file.OpenReadStream();
                var sig = new byte[8];
                int bytesRead = stream.Read(sig, 0, 8);
                if (bytesRead < 4) return false;

                // JPEG: FF D8 FF
                if (sig[0] == 0xFF && sig[1] == 0xD8 && sig[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47
                if (sig[0] == 0x89 && sig[1] == 0x50 && sig[2] == 0x4E && sig[3] == 0x47)
                    return true;

                // GIF: 47 49 46
                if (sig[0] == 0x47 && sig[1] == 0x49 && sig[2] == 0x46)
                    return true;

                // WebP / RIFF: 52 49 46 46
                if (sig[0] == 0x52 && sig[1] == 0x49 && sig[2] == 0x46 && sig[3] == 0x46)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
