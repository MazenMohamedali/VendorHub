namespace VendorHub.DTOs.ProductDto
{
    public class ProductAdminDetailsDto : ProductDetailsWithStatusDto
    {
        public int VendorId { get; set; }
        public int CategoryId { get; set; }
    }
}
