using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VendorHub.DTOs.CategoryDto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        private string? _normalizedName;

        [StringLength(100, MinimumLength = 2)]
        public string? NormalizedName
        {
            get => string.IsNullOrWhiteSpace(_normalizedName) ? Name.ToLower() : _normalizedName;
            set => _normalizedName = value?.ToLower();
        }

        [Required(ErrorMessage = "Category image is required")]
        public IFormFile ImageFile { get; set; } = null!;
    }
}
