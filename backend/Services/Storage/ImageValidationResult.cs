namespace VendorHub.Services.Storage
{
    public class ImageValidationResult
    {
        public bool IsValid { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
