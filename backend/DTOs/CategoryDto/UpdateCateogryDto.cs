using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.CategoryDto
{
    public class UpdateCategoryDto
    {
        private string? _normalizedName;
        [StringLength(100, MinimumLength = 2)]
        public string? NormalizedName {
            get => _normalizedName;
            set => _normalizedName = value?.ToLower();
        }

        public IFormFile? ImageFile { get; set; }
        public bool? IsActive { get; set; }
    }
}
