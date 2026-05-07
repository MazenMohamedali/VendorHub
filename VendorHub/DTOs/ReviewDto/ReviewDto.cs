using VendorHub.Models;

namespace VendorHub.DTOs.ReviewDto
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }

        public static ReviewDto FromEntity(Review review, string customerName = "Customer")
        {
            return new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CustomerName = customerName,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
