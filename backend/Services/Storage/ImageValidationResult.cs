namespace VendorHub.Services.Storage
{
    public enum ImageValidationError
    {
        None = 0,
        NoFileUploaded,
        FileTooLarge,
        InvalidExtension,
        InvalidContentType,
        SignatureMismatch
    }

    public class ImageValidationResult
    {
        public bool IsValid { get; init; }
        public ImageValidationError ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public static ImageValidationResult Success() =>
            new() { IsValid = true, ErrorCode = ImageValidationError.None };

        public static ImageValidationResult Failure(ImageValidationError errorCode, string message) =>
            new() { IsValid = false, ErrorCode = errorCode, ErrorMessage = message };
    }
}
