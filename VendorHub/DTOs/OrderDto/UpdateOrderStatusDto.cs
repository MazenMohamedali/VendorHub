using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.OrderDto
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }
}
