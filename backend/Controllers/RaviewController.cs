using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.ReviewDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{productId:int}")]
        public async Task<ActionResult<GeneralResponse<ReviewDto?>>> AddReview(CreateReviewDto reviewDto, int productId, CancellationToken cancellationToken = default)
        {
            int userId = this.GetUserId();    
            var result = await _reviewService.AddReviewAsync(productId, userId, reviewDto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("{productId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<GeneralResponse<PagedResult<ReviewDto>>>> GetReviews(int productId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
