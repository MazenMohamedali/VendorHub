using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #region Vendor Dashboard
        [HttpGet("vendor-orders")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<PagedResult<VendorOrderDto>>>> GetVendorOrders(int page = 1, int pageSize = 10, string? statusFilter = null, CancellationToken cancellationToken = default)
        {
            var vendorId = this.GetUserId();
            var result = await _orderService.GetVendorOrdersAsync(vendorId, page, pageSize, statusFilter, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("vendor-orders/{orderId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorOrderDto>>> GetVendorOrderById(int orderId, CancellationToken cancellationToken = default)
        {
            var vendorId = this.GetUserId();
            var result = await _orderService.GetVendorOrderByIdAsync(orderId, vendorId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPatch("{orderId:int}/status")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse>> UpdateOrderStatus(int orderId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
        {
            var vendorId = this.GetUserId();
            var result = await _orderService.UpdateOrderStatusAsync(orderId, vendorId, dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("vendor-orders-stats")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorOrdersStatsDto>>> GetVendorOrdersStats(CancellationToken cancellationToken = default)
        {
            var vendorId = this.GetUserId();
            var result = await _orderService.GetVendorOrdersStatsAsync(vendorId, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Customer
        [HttpPost]
        public async Task<ActionResult<GeneralResponse<OrderDetailsDto>>> CreateOrder(CreateOrderDto dto, CancellationToken cancellationToken = default)
        {
            int customerId = this.GetUserId();
            var result = await _orderService.CreateOrderAsync(dto, customerId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<GeneralResponse<OrderDetailsDto>>> GetOrder(int orderId, CancellationToken cancellationToken = default)
        {
            var customerId = this.GetUserId();
            var result = await _orderService.GetOrderAsync(orderId, customerId, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<OrderDetailsDto>>>> GetCustomerOrders(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var customerId = this.GetUserId();
            var result = await _orderService.GetCustomerOrdersAsync(customerId, pageNumber, pageSize, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion
    }
}
