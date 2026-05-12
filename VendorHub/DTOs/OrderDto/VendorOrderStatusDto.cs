namespace VendorHub.DTOs.OrderDto
{
    public class VendorOrdersStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
    }
}
