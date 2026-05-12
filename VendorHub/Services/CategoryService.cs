using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.CategoryDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGeneralRepository<Category> _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryService(IGeneralRepository<Category> categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Filter Categories
        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetFilteredCategoriesAsync(Expression<Func<Category, bool>> filter, string errorMessage = "No categories found")
        {
            var categories = await _categoryRepository
                .GetAll()
                .Where(filter)
                .Select(CategoryToDetailsDto()) // Maps and calculates ProductCount in 1 SQL query
                .ToListAsync();

            if (categories == null || !categories.Any())
                return new GeneralResponse<IEnumerable<CategoryDetailsDto>>().Failed(errorMessage);

            return new GeneralResponse<IEnumerable<CategoryDetailsDto>>().Succeeded(categories);
        }

        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> SearchByNameAsync(string name)
        {
            return await GetFilteredCategoriesAsync(c => c.Name.Contains(name) && c.IsActive, $"No categories found matching: {name}");
        }
        #endregion

        #region Command Methods (Add, Update, Delete)
        public async Task<GeneralResponse<CategoryDetailsDto>> AddAsync(CreateCategoryDto dto)
        {
            var exists = await _categoryRepository.GetAll().AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());
            if (exists) return new GeneralResponse<CategoryDetailsDto>().Failed("Category already exists");

            var category = new Category { Name = dto.Name };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();

            if (dto.ImageFile != null) await AddImageToCategoryAsync(dto.ImageFile, category);

            var result = CategoryToDetailsDto().Compile()(category);
            return new GeneralResponse<CategoryDetailsDto>().Succeeded(result, "Category created successfully");
        }

        private async Task AddImageToCategoryAsync(IFormFile imageFile, Category category)
        {
            string extension = System.IO.Path.GetExtension(imageFile.FileName);
            string fileName = $"{category.Id}{extension}";
            string imagesFolder = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Categories");

            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            string filePath = System.IO.Path.Combine(imagesFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            category.ImageUrl = fileName;
            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveAsync();
        }

        public async Task<GeneralResponse<CategoryDetailsDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return new GeneralResponse<CategoryDetailsDto>().Failed("Category not found");

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.ToLower() != category.Name.ToLower())
            {
                var exists = await _categoryRepository.GetAll().AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);
                if (exists) return new GeneralResponse<CategoryDetailsDto>().Failed("New category name already exists");
                category.Name = dto.Name;
            }

            if (dto.ImageUrl != null) category.ImageUrl = dto.ImageUrl;
            if (dto.IsActive.HasValue) category.IsActive = dto.IsActive.Value;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveAsync();

            var result = CategoryToDetailsDto().Compile()(category);
            return new GeneralResponse<CategoryDetailsDto>().Succeeded(result, "Category updated successfully");
        }

        public async Task<GeneralResponse> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return new GeneralResponse().Failed("Category not found");

            category.IsActive = false;
            await _categoryRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Category deactivated successfully");
        }

        public async Task<GeneralResponse> HardDeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return new GeneralResponse().Failed("Category not found");

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Category permanently removed");
        }
        #endregion

        #region Get Methods
        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var categories = await _categoryRepository.GetAll()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(CategoryToDetailsDto())
                .ToListAsync();

            return new GeneralResponse<IEnumerable<CategoryDetailsDto>>().Succeeded(categories);
        }

        public async Task<GeneralResponse<IEnumerable<CategoryDetailsDto>>> GetActiveAsync()
        {
            return await GetFilteredCategoriesAsync(c => c.IsActive);
        }

        public async Task<GeneralResponse<CategoryDetailsDto>> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetAll()
                .Where(c => c.Id == id)
                .Select(CategoryToDetailsDto())
                .FirstOrDefaultAsync();

            return category == null
                ? new GeneralResponse<CategoryDetailsDto>().Failed("Category not found")
                : new GeneralResponse<CategoryDetailsDto>().Succeeded(category);
        }
        #endregion

        #region Private Helpers
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
                ProductCount = c.Products.Count(p => p.Status == ProductStatus.REVIEWED)
            };
        }
        #endregion
    }
}