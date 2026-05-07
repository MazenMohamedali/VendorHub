using VendorHub.Models;

namespace VendorHub.DTOs.ProductDto
{
    public class ProductDetailsWithStatusDto : ProductDetailsDto
    {
        public ProductStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ReviewCount { get; set; }
    }
}
