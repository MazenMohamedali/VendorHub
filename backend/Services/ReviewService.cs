using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VendorHub.DTOs.ReviewDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IGeneralRepository<Review> _reviewRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Order> _orderRepository;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IGeneralRepository<Review> reviewRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<Order> orderRepository,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<GeneralResponse<ReviewDto?>> AddReviewAsync(int productId, int customerId, CreateReviewDto reviewDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to post review for Product {ProductId} by Customer {CustomerId}", productId, customerId);

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                return GeneralResponse<ReviewDto?>.NotFound("Target product not found.");

            if (!await IsCustomerOrderProductAsync(customerId, productId, cancellationToken))
                return GeneralResponse<ReviewDto?>.Forbidden("You can only review products you have ordered.");
            
            bool alreadyReviewed = await _reviewRepository
                            .GetAllAsNoTracking()
                            .AnyAsync(r => r.ProductId == productId && r.CustomerId == customerId, cancellationToken);

            if (alreadyReviewed)
            {
                return GeneralResponse<ReviewDto?>.InvalidInput("You have already submitted a review for this product.");
            }

            var review = reviewDto.ToEntity(productId, customerId);

            try
            {
                await _reviewRepository.AddAsync(review, cancellationToken);

                product.ReviewCount++;
                product.OverallStars += review.Rating;

                await _reviewRepository.SaveAsync(cancellationToken);

                _logger.LogInformation("Successfully posted review {ReviewId} for Product {ProductId}", review.Id, productId);

                var compiledReview = await _reviewRepository
                    .GetByAsNoTracking(r => r.Id == review.Id)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.SecondName}" : "Anonymous",
                        CreatedAt = r.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return GeneralResponse<ReviewDto?>.Created(compiledReview, "Review posted successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency collision when updating rating for Product {ProductId}", productId);
                return GeneralResponse<ReviewDto?>.Error("A concurrency conflict occurred. Please try submitting your review again.");
            }
        }

        public async Task<GeneralResponse<PagedResult<ReviewDto>>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _reviewRepository
                .GetAllAsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToPagedResultAsync(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.SecondName}" : "Anonymous",
                    CreatedAt = r.CreatedAt
                }, page, pageSize, cancellationToken);

            return GeneralResponse<PagedResult<ReviewDto>>.Succeeded(pagedResult, "Reviews retrieved successfully.");
        }

        private Task<bool> IsCustomerOrderProductAsync(int customerId, int productId, CancellationToken cancellationToken)
        {
            return _orderRepository
                .GetAllAsNoTracking()
                .AnyAsync(o => o.CustomerId == customerId && o.Items.Any(i => i.ProductId == productId), cancellationToken);
        }
    }
}
