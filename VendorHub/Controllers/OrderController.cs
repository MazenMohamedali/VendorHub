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
