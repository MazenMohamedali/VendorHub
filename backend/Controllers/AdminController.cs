using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProductService _productService;
        public AdminController(IProductService productService) => _productService = productService;

        #region Admin Actions
        [HttpGet("{id:int}/admin")]
        public async Task<ActionResult<GeneralResponse<ProductAdminDetailsDto>>> GetProductDetailsForAdmin(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductDetailsForAdminAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("admin/all")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductAdminDetailsDto>>>> GetAllProductsForAdmin(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetAllProductsForAdminAsync(page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPatch("{id:int}/approve")]
        public async Task<ActionResult<GeneralResponse>> Approve(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.ApproveProductAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPatch("{id:int}/reject")]
        public async Task<ActionResult<GeneralResponse>> Reject(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.RejectProductAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion
    }
}
