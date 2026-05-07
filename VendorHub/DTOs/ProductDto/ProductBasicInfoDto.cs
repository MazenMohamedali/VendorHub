namespace VendorHub.DTOs.ProductDto
{
    public class ProductBasicInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImgUrl { get; set; }
    }
}
