using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.StatisticsDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IGeneralRepository<OrderItem> _orderItemRepository;
        private readonly IGeneralRepository<Product> _productRepository;

        public StatisticsService(IGeneralRepository<OrderItem> orderItemRepository, IGeneralRepository<Product> productRepository)
        {
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        public async Task<GeneralResponse<VendorStatisticsDto>> GetVendorStatisticsAsync(int vendorId)
        {
            var vendorProducts = await _productRepository
                .GetAll()
                .Where(p => p.VendorId == vendorId)
                .ToListAsync();

            var productIds = vendorProducts.Select(p => p.Id).ToList();

            List<OrderItem> orderItems = await _orderItemRepository
                .GetAll()
                .Where(oi => productIds.Contains(oi.ProductId))
                .Include(oi => oi.Order)
                .ToListAsync();

            decimal totalRevenue = orderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);

            int totalOrders = orderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();

            var averageRating = vendorProducts
                .Any(p => p.ReviewCount > 0) ?
                vendorProducts
                    .Where(p => p.ReviewCount > 0)
                    .Average(p => (double)p.OverallStars / p.ReviewCount)
                : 0;

            var topProducts = vendorProducts
                .Select(p => new ProductPerformanceDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitsSold = orderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => oi.Quantity),
                    Revenue = orderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                    AverageRating = p.ReviewCount > 0 ? (double)p.OverallStars / p.ReviewCount : 0,
                    ReviewCount = p.ReviewCount
                })
                .OrderByDescending(pp => pp.Revenue)
                .Take(10)
                .ToList();


            var monthlySales = Enumerable.Range(0, 12)
                .Select(i => new
                {
                    Month = DateTime.Now.AddMonths(-11 + i),
                    Items = orderItems.Where(oi =>
                        oi.Order.CreatedAt.Year == DateTime.Now.AddMonths(-11 + i).Year &&
                        oi.Order.CreatedAt.Month == DateTime.Now.AddMonths(-11 + i).Month)
                })
                .Select(g => new MonthlySalesDto
                {
                    Month = g.Month.ToString("MMMM yyyy"),
                    Revenue = g.Items.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                    Orders = g.Items.Select(oi => oi.OrderId).Distinct().Count()
                })
                .ToList();


            var vendorStatistics = new VendorStatisticsDto
            {
                TotalSales = orderItems.Sum(oi => oi.Quantity),
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = vendorProducts.Count,
                AverageProductRating = averageRating,
                TopProducts = topProducts,
                MonthlySales = monthlySales
            };

            return new GeneralResponse<VendorStatisticsDto>().Succeeded(vendorStatistics);
        }
    }
}
