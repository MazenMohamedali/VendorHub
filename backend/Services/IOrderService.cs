using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface IOrderService
    {
        Task<GeneralResponse<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto dto, int customerId, CancellationToken cancellationToken = default);
        Task<GeneralResponse<List<OrderStatusInfoDto>>> GetAvailableStatusesAsync(CancellationToken cancellationToken = default);
        Task<GeneralResponse<IEnumerable<OrderDetailsDto>>> GetCustomerOrdersAsync(int customerId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<GeneralResponse<OrderDetailsDto>> GetOrderAsync(int orderId, int customerId, CancellationToken cancellationToken = default);
        Task<GeneralResponse<VendorOrderDto>> GetVendorOrderByIdAsync(int orderId, int vendorId, CancellationToken cancellationToken = default);
        Task<GeneralResponse<PagedResult<VendorOrderDto>>> GetVendorOrdersAsync(int vendorId, int page, int pageSize, string? statusFilter = null, CancellationToken cancellationToken = default);
        Task<GeneralResponse<VendorOrdersStatsDto>> GetVendorOrdersStatsAsync(int vendorId, CancellationToken cancellationToken = default);
        Task<GeneralResponse> UpdateOrderStatusAsync(int orderId, int vendorId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);
    }
}
