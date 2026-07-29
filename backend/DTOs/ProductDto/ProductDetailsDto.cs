namespace VendorHub.DTOs.ProductDto
{
    public class ProductDetailsDto : ProductCardDto
    {
        public long UnitsInStock { get; set; } 
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string? storeName { get; set; }
        public string? CategoryName { get; set; }
    }
}
