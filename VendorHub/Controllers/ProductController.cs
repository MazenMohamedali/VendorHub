using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using VendorHub.Attributes;
using VendorHub.DTOs.ProductDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        #region Searching
        [HttpGet("search-name")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> SearchByName(string name)
        {
            var result = await _productService.SearchByNameAsync(name);
            return await wrappingResult(result);
        }

        [HttpGet("search-category")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> SearchByCategory(string category)
        {
            var result = await _productService.SearchByCategoryAsync(category);
            return await wrappingResult(result);
        }

        [HttpGet("search-price")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> SearchByPrice(long min, long max)
        {
            var result = await _productService.SearchByPriceAsync(min, max);
            return await wrappingResult(result);
        }
        #endregion

        #region Customer Action(getter)
        [HttpGet("list")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> GetPublicProducts()
        {
            return Ok(await _productService.GetProductsCardAsync());
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductCardDto>>>> GetProductsByCategory(int categoryId)
        {
            var result = await _productService.GetProductsCardByCategoryId(categoryId);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}/customer")]
        public async Task<ActionResult<GeneralResponse<ProductDetailsDto>>> GetDetails(int id)
        {
            var result = await _productService.GetProductDetailsForCustomerAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        #endregion
        
        #region Vendor Actions
        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [RequirePermission(PermissionType.CanUploadProducts)]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> Add(AddProductDto dto)
        {
            var result = await _productService.AddAsync(dto);
            if (result.Success)
                return CreatedAtAction(nameof(GetDetailsForVendor), new { id = result.Data?.Id }, result);
            return BadRequest(result);
        }

        [HttpGet("my-products")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductDetailsWithStatusDto>>>> GetMyProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _productService.GetVendorProductsListAsync(int.Parse(userId!));
            return Ok(result);
        }

        [HttpGet("{id}/vendor")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> GetDetailsForVendor(int id)
        {
            var result = await _productService.GetProductDetailsForVendorAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")]
        [RequirePermission(PermissionType.CanEditProducts)]
        public async Task<ActionResult<GeneralResponse<ProductDetailsWithStatusDto>>> Update(int id, EditProductDto dto)
        {
            var result = await _productService.UpdateAsync(dto, id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        #endregion

        #region Admin Actions
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/admin")]
        public async Task<ActionResult<GeneralResponse<ProductAdminDetailsDto>>> GetProductDetailsForAdmin(int id)
        {
            var result = await _productService.GetProductDetailsForAdminAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<ProductAdminDetailsDto>>>> GetAllProductsForAdmin()
        {
            var result = await _productService.GetAllProductsForAdminAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/approve")]
        public async Task<ActionResult<GeneralResponse>> Approve(int id)
        {
            var result = await _productService.ApproveProductAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/reject")]
        public async Task<ActionResult<GeneralResponse>> Reject(int id)
        {
            var result = await _productService.RejectProductAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        } 
        #endregion

        #region Shared Actions
        [Authorize(Roles = "Vendor,Admin")]
        [RequirePermission(PermissionType.CanDeleteProducts)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<GeneralResponse>> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        #endregion

        #region Helper Methods
        private async Task<ActionResult<GeneralResponse<T>>> wrappingResult<T>(GeneralResponse<T> result)
        {
            if (result.Success)
                return Ok(result);

            return NotFound(result);
        }
        #endregion

    }
}
