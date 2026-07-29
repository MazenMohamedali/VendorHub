using System.ComponentModel.DataAnnotations;
using VendorHub.Models;
using VendorHub.Validation;

namespace VendorHub.DTOs.ProductDto
{
    public class AddProductDto : ProductAddEditBase
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }


        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public long Quantity { get; set; }


        [Required(ErrorMessage = "Select a product image")]
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }


        [Required(ErrorMessage = "Select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }


        public int VendorId { get; set; }

        public Product GetProduct()
        {
            return new Product
            {
                Name = Name,
                Price = Price,
                Quantity = Quantity,
                CategoryId = CategoryId,
                VendorId = VendorId,
                ProductionDate = ProductionDate,
                ExpireDate = ExpireDate,
                ImgUrl = "",
                Status = ProductStatus.PENDING
            };
        }
    }
}
