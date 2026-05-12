using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
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

        #region new Endpoints
        [HttpGet("vendor-orders")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<PagedResult<VendorOrderDto>>>> GetVendorOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? statusFilter = null)
        {
            var vendorId = GetUserId();
            var result = await _orderService.GetVendorOrdersAsync(vendorId, page, pageSize, statusFilter);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("vendor-orders/{orderId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorOrderDto>>> GetVendorOrderById(int orderId)
        {
            var vendorId = GetUserId();
            var result = await _orderService.GetVendorOrderByIdAsync(orderId, vendorId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPatch("{orderId}/status")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse>> UpdateOrderStatus(
            int orderId,
            [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vendorId = GetUserId();
            var result = await _orderService.UpdateOrderStatusAsync(orderId, vendorId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("vendor-orders-stats")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<VendorOrdersStatsDto>>> GetVendorOrdersStats()
        {
            var vendorId = GetUserId();
            var result = await _orderService.GetVendorOrdersStatsAsync(vendorId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
        #endregion


        [HttpPost]
        public async Task<ActionResult<GeneralResponse<OrderDetailsDto>>> CreateOrder(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new GeneralResponse().Failed("User not authenticated"));

            int customerId = int.Parse(userIdClaim);
            var result = await _orderService.CreateOrderAsync(dto, customerId);

            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new GeneralResponse().Failed("User not authenticated"));

            int customerId = int.Parse(userIdClaim);
            var result = await _orderService.GetOrderAsync(orderId, customerId);

            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet("my-orders")]
        public async Task<IActionResult> GetCustomerOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new GeneralResponse().Failed("User not authenticated"));

            int customerId = int.Parse(userIdClaim);
            var result = await _orderService.GetCustomerOrdersAsync(customerId, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
