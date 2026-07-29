using System.Linq.Expressions;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface ICategoryService
    {
        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> SearchByNameAsync(
                    string name,
                    CancellationToken cancellationToken = default);

        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetFilteredCategoriesAsync(
            Expression<Func<Category, bool>> filter,
            CancellationToken cancellationToken = default,
            string errorMessage = "No categories found");

        Task<GeneralResponse<CategoryDetailsDto>> AddAsync(
            CreateCategoryDto dto,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<CategoryDetailsDto>> UpdateAsync(
            int id,
            UpdateCategoryDto dto,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> HardDeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetActiveAsync(
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<CategoryDetailsDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
