namespace VendorHub.Settings
{
    public class ImageStorageOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
        public string[] AllowedExtensions { get; set; } =
            new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        public string[] AllowedContentTypes { get; set; } =
            new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
    }
}
