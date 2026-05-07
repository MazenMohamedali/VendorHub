using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.CategoryDto
{
    public class UpdateCategoryDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }
}
