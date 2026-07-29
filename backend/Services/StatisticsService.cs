using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.StatisticsDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;

namespace VendorHub.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IGeneralRepository<OrderItem> _orderItemRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IGeneralRepository<OrderItem> orderItemRepository,
            IGeneralRepository<Product> productRepository,
            ICacheService cacheService,
            ILogger<StatisticsService> logger)
        {
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GeneralResponse<VendorStatisticsDto>> GetVendorStatisticsAsync(int vendorId, CancellationToken cancellationToken)
        {
            var stats = await (_cacheService.GetOrSetAsync(
                key: CacheKeys.VendorStats(vendorId),
                factory: () =>
                {
                    return VendorStatsAsync(vendorId, cancellationToken);
                },
                expiration: CacheKeys.VendorStats_TTL,
                cancellationToken: cancellationToken
            ));

            _logger.LogInfoWithContext("Analytics compiled successfully for vendor {VendorId}", new { VendorId = vendorId });

            return GeneralResponse<VendorStatisticsDto>.Succeeded(stats!, "Vendor analytics compiled successfully.");
        }

        private async Task<VendorStatisticsDto> VendorStatsAsync(int vendorId, CancellationToken cancellationToken)
        {
            var vendorProductsQuery = _productRepository.GetBy(p => p.VendorId == vendorId);
            var vendorItemsQuery = _orderItemRepository.GetBy(oi => oi.Product.VendorId == vendorId);

            var kpiData = await vendorItemsQuery
                .GroupBy(oi => 1)
                .Select(g => new
                {
                    TotalSales = g.Sum(oi => (int?)oi.Quantity) ?? 0,
                    TotalRevenue = g.Sum(oi => (decimal?)(oi.PriceAtPurchase * oi.Quantity)) ?? 0,
                    TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            int totalSales = kpiData?.TotalSales ?? 0;
            decimal totalRevenue = kpiData?.TotalRevenue ?? 0;
            int totalOrders = kpiData?.TotalOrders ?? 0;

            var productMetrics = await vendorProductsQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalProducts = g.Count(),
                    AverageRating = g.Where(p => p.ReviewCount > 0)
                                     .Average(p => (double?)((double)p.OverallStars / p.ReviewCount)) ?? 0
                })
                .FirstOrDefaultAsync(cancellationToken);

            int totalProducts = productMetrics?.TotalProducts ?? 0;
            double averageRating = productMetrics?.AverageRating ?? 0;

            var topProducts = await vendorProductsQuery
                .Select(p => new ProductPerformanceDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitsSold = p.OrderItems.Sum(oi => (int?)oi.Quantity) ?? 0,
                    Revenue = p.OrderItems.Sum(oi => (decimal?)(oi.PriceAtPurchase * oi.Quantity)) ?? 0,
                    AverageRating = p.ReviewCount > 0 ? (double)p.OverallStars / p.ReviewCount : 0,
                    ReviewCount = p.ReviewCount
                })
                .OrderByDescending(pp => pp.Revenue)
                .Take(10)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

            var rawMonthlyData = await vendorItemsQuery
                .Where(oi => oi.Order.CreatedAt >= startDate)
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.Quantity,
                    oi.PriceAtPurchase,
                    Year = oi.Order.CreatedAt.Year,
                    Month = oi.Order.CreatedAt.Month
                })
                .ToListAsync(cancellationToken);

            var monthlySales = Enumerable.Range(0, 12)
                .Select(i => startDate.AddMonths(i))
                .Select(date =>
                {
                    var itemsInMonth = rawMonthlyData
                        .Where(x => x.Year == date.Year && x.Month == date.Month)
                        .ToList();

                    return new MonthlySalesDto
                    {
                        Month = date.ToString("MMMM yyyy"),
                        Revenue = itemsInMonth.Sum(x => x.PriceAtPurchase * x.Quantity),
                        Orders = itemsInMonth.Select(x => x.OrderId).Distinct().Count()
                    };
                })
                .ToList();

            return new VendorStatisticsDto
            {
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                AverageProductRating = averageRating,
                TopProducts = topProducts,
                MonthlySales = monthlySales
            };
        }
    }
}
