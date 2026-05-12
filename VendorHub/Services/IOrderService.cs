using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IOrderService
    {
        Task<GeneralResponse<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto dto, int customerId);
        Task<GeneralResponse<IEnumerable<OrderDetailsDto>>> GetCustomerOrdersAsync(int customerId, int pageNumber = 1, int pageSize = 10);
        Task<GeneralResponse<OrderDetailsDto>> GetOrderAsync(int orderId, int customerId);
        Task<decimal> QuantityAndTotalPriceHandle(Product product, CartItemDto cartItem);

        public Task<GeneralResponse> UpdateOrderStatusAsync(int orderId, int vendorId, UpdateOrderStatusDto dto);
        public Task<GeneralResponse<VendorOrdersStatsDto>> GetVendorOrdersStatsAsync(int vendorId);
        public Task<GeneralResponse<PagedResult<VendorOrderDto>>> GetVendorOrdersAsync(int vendorId, int page, int pageSize, string? statusFilter = null);
        public Task<GeneralResponse<VendorOrderDto>> GetVendorOrderByIdAsync(int orderId, int vendorId);
    }
}