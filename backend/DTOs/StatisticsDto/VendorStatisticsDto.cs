namespace VendorHub.DTOs.StatisticsDto
{
    public class VendorStatisticsDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public double AverageProductRating { get; set; }
        public List<ProductPerformanceDto> TopProducts { get; set; }
        public List<MonthlySalesDto> MonthlySales { get; set; }
    }
}
