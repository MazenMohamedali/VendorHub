using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.Constants;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;
using VendorHub.Services.Storage;

namespace VendorHub.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGeneralRepository<Category> _categoryRepository;
        private readonly ICacheService _cacheService;
        private readonly IFileService _fileService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IGeneralRepository<Category> categoryRepository,
            ICacheService cacheService,
            IFileService fileService,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
            _fileService = fileService;
            _logger = logger;
        }

        #region Filter Categories
        public Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(GeneralResponse<IEnumerable<CategoryDetailsDto>>.InvalidInput("Search term cannot be empty"));

            return GetFilteredCategoriesAsync(c => c.Name.Contains(name) && c.IsActive, cancellationToken, $"No categories found matching: {name}");
        }

        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetFilteredCategoriesAsync(Expression<Func<Category, bool>> filter, CancellationToken cancellationToken = default, string errorMessage = "No categories found")
        {
            var categories = await _categoryRepository
                .GetBy(filter)
                .Select(CategoryToDetailsDto())
                .ToListAsync(cancellationToken);
               

            foreach (var category in categories)
            {
                category.ImageUrl = _fileService.BuildImageUrl(ImageFolders.Categories, category.ImageUrl);
            }

            return GeneralResponse<IEnumerable<CategoryDetailsDto>>.Succeeded(categories);
        }
        #endregion

        #region Command Methods (Add, Update, Delete)
        public async Task<GeneralResponse<CategoryDetailsDto>> AddAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.NormalizedName))
                return GeneralResponse<CategoryDetailsDto>.InvalidInput("Category name is required.");

            string? savedImagePath = null;

            try
            {
                var category = new Category
                {
                    Name = dto.NormalizedName.Trim()
                };

                if (dto.ImageFile != null)
                {
                    savedImagePath = await _fileService.SaveImageAsync(dto.ImageFile, ImageFolders.Categories);
                    category.ImageUrl = savedImagePath;
                }

                await _categoryRepository.AddAsync(category, cancellationToken);
                await _categoryRepository.SaveAsync(cancellationToken);

                var result = MapToDetailsDto(category);

                _logger.LogInfoWithContext(
                    "Category created successfully with structural tracking assigned.",
                    new { CategoryId = category.Id, category.Name, HasImage = savedImagePath != null });

                await _cacheService.RemoveAsync(CacheKeys.ALL_CATEGORIES, cancellationToken);

                return GeneralResponse<CategoryDetailsDto>.Created(result, "Category created successfully");

            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                _logger.LogWarningWithContext("Conflict handled: Category name already exists.", new { Name = dto.NormalizedName });

                if (!string.IsNullOrEmpty(savedImagePath))
                    await TryDeleteFileAsync(savedImagePath);

                return GeneralResponse<CategoryDetailsDto>.InvalidInput("Category already exists.");
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext("Failed to create category. Rollback active.", ex, dto.NormalizedName);

                if (!string.IsNullOrEmpty(savedImagePath))
                    await TryDeleteFileAsync(savedImagePath);

                throw;
            }
        }

        public async Task<GeneralResponse<CategoryDetailsDto>> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return GeneralResponse<CategoryDetailsDto>.NotFound("Category not found");

            if (!string.IsNullOrWhiteSpace(dto.NormalizedName) && dto.NormalizedName.Trim() != category.Name)
            {
                bool exists = await _categoryRepository.ExistsAsync(c => c.Name == dto.NormalizedName.Trim() && c.Id != id, cancellationToken);
                if (exists)
                    return GeneralResponse<CategoryDetailsDto>.InvalidInput("New category name already exists");

                category.Name = dto.NormalizedName.Trim();
            }

            string? oldImagePath = category.ImageUrl;
            string? newImagePath = null;

            try
            {
                if (dto.ImageFile != null)
                {
                    newImagePath = await _fileService.ReplaceImageAsync(category.ImageUrl, dto.ImageFile, ImageFolders.Categories);
                    category.ImageUrl = newImagePath;
                }

                if (dto.IsActive.HasValue)
                    category.IsActive = dto.IsActive.Value;

                category.UpdatedAt = DateTime.UtcNow;

                _categoryRepository.Update(category);
                await _categoryRepository.SaveAsync(cancellationToken);

                var result = MapToDetailsDto(category);

                _logger.LogInfoWithContext(
                    "Category ID: {CategoryId} updated fully inside storage systems.",
                    new { CategoryId = id, Name = dto.NormalizedName, IsActive = dto.IsActive, HasNewImage = dto.ImageFile != null },
                    id);

                await _cacheService.RemoveAsync(CacheKeys.ALL_CATEGORIES, cancellationToken);
                await _cacheService.RemoveAsync(CacheKeys.CategoryDetails(id), cancellationToken);

                return GeneralResponse<CategoryDetailsDto>.Succeeded(result, "Category updated successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarningWithContext("Concurrency conflict triggered on Category Update.", ex, id);
                return GeneralResponse<CategoryDetailsDto>.Error("The category was updated by another administrator. Please refresh and try again.");
            }
        }

        public async Task<GeneralResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return GeneralResponse.NotFound("Category not found");

            bool hasActiveProducts = await _categoryRepository.GetAllAsNoTracking()
                .Where(c => c.Id == id)
                .SelectMany(c => c.Products)
                .AnyAsync(p => p.Status == ProductStatus.REVIEWED, cancellationToken);

            if (hasActiveProducts)
                return GeneralResponse.InvalidInput("Cannot deactivate a category that contains active products.");

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.ALL_CATEGORIES, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.CategoryDetails(id), cancellationToken);

            return GeneralResponse.Succeeded("Category deactivated successfully");
        }

        public async Task<GeneralResponse> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return GeneralResponse.NotFound("Category not found");

            bool hasProducts = await _categoryRepository.GetAllAsNoTracking()
                .Where(c => c.Id == id)
                .SelectMany(c => c.Products)
                .AnyAsync(cancellationToken);

            if (hasProducts)
                return GeneralResponse.InvalidInput("Cannot permanently delete a category that contains active or historical product references.");

            if (!string.IsNullOrEmpty(category.ImageUrl))
                await _fileService.DeleteImageAsync(ImageFolders.Categories, category.ImageUrl);

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.ALL_CATEGORIES, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.CategoryDetails(id), cancellationToken);

            return GeneralResponse.Succeeded("Category permanently removed");
        }
        #endregion

        #region Get Methods
        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var categories = await _categoryRepository.GetAll()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(CategoryToDetailsDto())
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                category.ImageUrl = _fileService.BuildImageUrl(ImageFolders.Categories, category.ImageUrl);
            }

            return GeneralResponse<IEnumerable<CategoryDetailsDto>>.Succeeded(categories);
        }

        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryRepository
                .GetBy(c => c.IsActive)
                .Select(CategoryToDetailsDto())
                .ToCachedListAsync(_cacheService, CacheKeys.ALL_CATEGORIES, CacheKeys.CategoriesL2_TTL, cancellationToken);

            foreach (var category in categories)
            {
                category.ImageUrl = _fileService.BuildImageUrl(ImageFolders.Categories, category.ImageUrl);
            }

            return GeneralResponse<IEnumerable<CategoryDetailsDto>>.Succeeded(categories);
        }

        public async Task<GeneralResponse<CategoryDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetAll()
                .Where(c => c.Id == id)
                .Select(CategoryToDetailsDto())
                .ToCachedFirstOrDefaultAsync(_cacheService, CacheKeys.CategoryDetails(id), CacheKeys.CategoriesL2_TTL, cancellationToken);

            if (category != null)
            {
                category.ImageUrl = _fileService.BuildImageUrl(ImageFolders.Categories, category.ImageUrl);
            }

            return category == null
                ? GeneralResponse<CategoryDetailsDto>.NotFound("Category not found")
                : GeneralResponse<CategoryDetailsDto>.Succeeded(category);
        }
        #endregion

        #region Private File System Helpers

        private Expression<Func<Category, CategoryDetailsDto>> CategoryToDetailsDto()
        {
            return c => new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ProductCount = c.ProductCount
            };
        }

        private CategoryDetailsDto MapToDetailsDto(Category c)
        {
            return new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = _fileService.BuildImageUrl(ImageFolders.Categories, c.ImageUrl),
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ProductCount = c.ProductCount
            };
        }

        private async Task TryDeleteFileAsync(string path)
        {
            try
            {
                await _fileService.DeleteImageAsync(ImageFolders.Categories, path);
            }
            catch (Exception fileEx)
            {
                _logger.LogErrorWithContext("Zombie file cleanup failed during execution rollback paths.", fileEx, path);
            }
        }
        #endregion
    }
}
