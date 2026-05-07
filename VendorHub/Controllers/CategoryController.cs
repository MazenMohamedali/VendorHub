using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
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
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> Search([FromQuery] string searchTerm)
        {
            var result = await _categoryService.SearchByNameAsync(searchTerm);
            return await wrappingResult(result);
        }
        #endregion

        #region Public Actions
        [HttpGet("active")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> GetActive()
        {
            var result = await _categoryService.GetActiveAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        #endregion

        #region Admin Actions
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> Create(CreateCategoryDto dto)
        {
            var result = await _categoryService.AddAsync(dto);
            if (result.Success)
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);

            return BadRequest(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse<IEnumerable<CategoryDetailsDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _categoryService.GetAllAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse<CategoryDetailsDto>>> Update(int id, UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse>> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}/hard")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GeneralResponse>> HardDelete(int id)
        {
            var result = await _categoryService.HardDeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
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