using VendorHub.DTOs.ProductDto;

namespace VendorHub.DTOs.OrderDto
{
    public class OrderItemDto : ProductBasicInfoDto
    {
        public int Quantity { get; set; }
    }
}
