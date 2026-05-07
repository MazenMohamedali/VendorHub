using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using VendorHub.Models;

namespace VendorHub.DTOs.ProductDto
{
    public class EditProductDto :  ProductAddEditBase
    {

        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }


        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }


        [Range(0, long.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public long? Quantity { get; set; }


        [Display(Name = "Category")]
        public int? CategoryId { get; set; }


        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        public async Task ApplyTo(Product product)
        {
            if (!string.IsNullOrEmpty(Name))
                product.Name = Name;

            if (Price.HasValue && Price > 0)
                product.Price = Price.Value;

            if (Quantity.HasValue && Quantity > 0)
                product.Quantity = Quantity.Value;

            if (CategoryId.HasValue && CategoryId > 0)
                product.CategoryId = CategoryId.Value;

            if (ProductionDate.HasValue)
                product.ProductionDate = ProductionDate.Value;

            if (ExpireDate.HasValue)
                product.ExpireDate = ExpireDate.Value;
        }
    }
}
