
namespace VendorHub.Services.Storage
{
    public interface IImageValidator
    {
        Task<ImageValidationResult> ValidateAsync(IFormFile file);
    }
}
