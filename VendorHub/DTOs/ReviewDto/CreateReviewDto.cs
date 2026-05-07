using System.ComponentModel.DataAnnotations;
using VendorHub.Models;

namespace VendorHub.DTOs.ReviewDto
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public Review ToEntity(int productId, int customerId)
        {
            return new Review
            {
                ProductId = productId,
                CustomerId = customerId,
                Rating = this.Rating,
                Comment = this.Comment,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
