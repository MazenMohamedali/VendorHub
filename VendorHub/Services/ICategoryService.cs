using System.Linq.Expressions;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface ICategoryService
    {
        Task<GeneralResponse<CategoryDetailsDto>> AddAsync(CreateCategoryDto dto);
        Task<GeneralResponse> DeleteAsync(int id);
        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetActiveAsync();
        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<GeneralResponse<CategoryDetailsDto>> GetByIdAsync(int id);
        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetFilteredCategoriesAsync(Expression<Func<Category, bool>> filter, string errorMessage = "No categories found");
        Task<GeneralResponse> HardDeleteAsync(int id);
        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> SearchByNameAsync(string name);
        Task<GeneralResponse<CategoryDetailsDto>> UpdateAsync(int id, UpdateCategoryDto dto);
    }
}