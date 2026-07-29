using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.Attributes;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("hot-products")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> GetHotProducts(int count = 6, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetHotProductsAsync(count, cancellationToken);
            return this.HandleResult(result);
        }

        #region Searching & Filtering
        [HttpGet("search-name")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductCardDto>>>> SearchByName(string name, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.SearchByNameAsync(name, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("search-category")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductCardDto>>>> SearchByCategory(string category, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.SearchByCategoryAsync(category, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("search-price")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductCardDto>>>> SearchByPrice(long min, long max, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.SearchByPriceAsync(min, max, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Customer Actions
        [HttpGet("list")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductCardDto>>>> GetPublicProducts(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductsCardAsync(page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductCardDto>>>> GetProductsByCategory(int categoryId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductsCardByCategoryIdAsync(categoryId, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("{id:int}/customer")]
        public async Task<ActionResult<GeneralResponse<ProductDetailsDto>>> GetDetails(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductDetailsForCustomerAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Vendor Storefront Management
        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [RequirePermission(PermissionType.CanUploadProducts)]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> Add([FromForm] AddProductDto dto, CancellationToken cancellationToken = default)
        {
            dto.VendorId = this.GetUserId();
            var result = await _productService.AddAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("my-products")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<PagedResult<ProductDetailsWithStatusDto>>>> GetMyProducts(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var userId = this.GetUserId();
            var result = await _productService.GetVendorProductsListAsync(userId, page, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("{id:int}/vendor")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> GetDetailsForVendor(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductDetailsForVendorAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Vendor")]
        [RequirePermission(PermissionType.CanEditProducts)]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> Update(int id, [FromForm] EditProductDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _productService.UpdateAsync(dto, id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Shared Actions
        [Authorize(Roles = "Vendor,Admin")]
        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionType.CanDeleteProducts)]
        public async Task<ActionResult<GeneralResponse>> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _productService.DeleteProductAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion
    }
}
