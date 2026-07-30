using SkiaSharp;

namespace VendorHub.Services.Storage
{
    public static class ImageCompressor
    {
        public static async Task CompressAndSaveImageAsync(IFormFile file, string targetFullPath, int maxWidth = 1200, int quality = 80)
        {
            await using var inputStream = file.OpenReadStream();
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
            {
                await using var fallbackStream = new FileStream(targetFullPath, FileMode.Create);
                await file.CopyToAsync(fallbackStream);
                return;
            }

            int targetWidth = original.Width;
            int targetHeight = original.Height;

            if (original.Width > maxWidth)
            {
                targetWidth = maxWidth;
                targetHeight = (int)((double)original.Height * maxWidth / original.Width);
            }

            using var resized = original.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized ?? original);
            using var data = image.Encode(SKEncodedImageFormat.Webp, quality);

            await using var outputStream = new FileStream(targetFullPath, FileMode.Create);
            data.SaveTo(outputStream);
        }
    }
}
