using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        #region Searching
        [HttpGet("search")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> Search(string searchTerm, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.SearchByNameAsync(searchTerm, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Public Actions
        [HttpGet("active")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> GetActive(CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetActiveAsync(cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetByIdAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion

        #region Admin Actions
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> Create([FromForm] CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.AddAsync(dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> GetAll(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetAllAsync(pageNumber, pageSize, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> Update(int id, [FromForm] UpdateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.UpdateAsync(id, dto, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse>> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.DeleteAsync(id, cancellationToken);
            return this.HandleResult(result);
        }

        [HttpDelete("{id}/hard")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse>> HardDelete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.HardDeleteAsync(id, cancellationToken);
            return this.HandleResult(result);
        }
        #endregion
    }
}
