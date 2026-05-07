using VendorHub.DTOs.ReviewDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface IReviewService
    {
        Task<GeneralResponse<ReviewDto?>> AddReviewAsync(int productId, int customerId, CreateReviewDto reviewDto);
        Task<GeneralResponse<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId);
    }
}