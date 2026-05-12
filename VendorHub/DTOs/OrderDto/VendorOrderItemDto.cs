namespace VendorHub.DTOs.OrderDto
{
    public class VendorOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal SubTotal => Quantity * PriceAtPurchase;
    }
}
