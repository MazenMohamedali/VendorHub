using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.OrderDto
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Delivery address is required")]
        [StringLength(500)]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Not a valid Egyptian phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Cart items required")]
        public IEnumerable<CartItemDto> CartItems { get; set; }
    }
}
