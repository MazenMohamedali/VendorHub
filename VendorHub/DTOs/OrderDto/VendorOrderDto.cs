namespace VendorHub.DTOs.OrderDto
{
    public class VendorOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VendorOrderItemDto> Items { get; set; } = new();
    }
}
