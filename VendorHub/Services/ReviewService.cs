using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VendorHub.DTOs.ReviewDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IGeneralRepository<Review> _reviewRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Customer> _customerRepository;
        private readonly IGeneralRepository<Order> _orderRepository;

        public ReviewService(IGeneralRepository<Review> reviewRepository, IGeneralRepository<Product> productRepository, IGeneralRepository<Customer> customerRepository, IGeneralRepository<Order> orderRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }

        public async Task<GeneralResponse<ReviewDto?>> AddReviewAsync(int productId, int customerId, CreateReviewDto reviewDto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (product == null || customer == null)
                return new GeneralResponse<ReviewDto?>().Failed("Product or Customer not found.");

            if(!await IsCustomerOrderProduct(customerId, productId))
                return new GeneralResponse<ReviewDto?>().Failed("You can only review products you have ordered.");

            var review = reviewDto.ToEntity(productId, customerId);

            try
            {
                await AddReview(review);
                await UpdateProductRatingAsync(product, review.Rating);

                string customerName = $"{customer.FirstName} {customer.SecondName}";

                var resultDto = ReviewDto.FromEntity(review, customerName);
                return new GeneralResponse<ReviewDto?>().Succeeded(resultDto, "Review added successfully.");
            }
            catch (Exception ex)
            {
                return new GeneralResponse<ReviewDto?>().Failed($"An error occurred: {ex.Message}");
            }
        }

        private async Task<bool> IsCustomerOrderProduct(int customerId, int productId)
        {
            return await _orderRepository
                .GetAll()
                .AnyAsync(o => o.CustomerId == customerId && o.Items.Any(i => i.ProductId == productId));
        }

        private async Task AddReview(Review review)
        {
            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveAsync();
        }

        private async Task UpdateProductRatingAsync(Product product, int stars)
        {
            product.ReviewCount++;
            product.OverallStars += stars;
            await _productRepository.SaveAsync();
        }

        public async Task<GeneralResponse<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _reviewRepository
                .GetAll()
                .Where(r => r.ProductId == productId)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CustomerName = $"{r.Customer.FirstName} {r.Customer.SecondName}",
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new GeneralResponse<IEnumerable<ReviewDto>>().Succeeded(reviews, "Reviews retrieved successfully.");
        }
    }
}
