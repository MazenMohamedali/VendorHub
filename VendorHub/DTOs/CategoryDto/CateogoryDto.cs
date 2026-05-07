using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.CategoryDto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Category image URL is required")]
        public string? ImageUrl { get; set; }
    }
}
