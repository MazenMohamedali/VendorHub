using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.ReviewDto;
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


        [HttpPost("{productId}")]
        public async Task<ActionResult<ReviewDto>> AddReview(CreateReviewDto reviewDto,int productId)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);    
            var result = await _reviewService.AddReviewAsync(productId, userId, reviewDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet("{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(int productId)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
