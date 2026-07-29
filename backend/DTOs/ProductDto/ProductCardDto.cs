namespace VendorHub.DTOs.ProductDto
{
    public class ProductCardDto : ProductBasicInfoDto
    {
        public double AverageStars { get; set; }
        public long ViewersNo { get; set; } = 0;
    }
}
