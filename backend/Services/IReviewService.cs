using VendorHub.DTOs.ReviewDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface IReviewService
    {
        Task<GeneralResponse<ReviewDto?>> AddReviewAsync(int productId, int customerId, CreateReviewDto reviewDto, CancellationToken cancellationToken = default);
        Task<GeneralResponse<PagedResult<ReviewDto>>> GetProductReviewsAsync(int productId, int page = 1,int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
