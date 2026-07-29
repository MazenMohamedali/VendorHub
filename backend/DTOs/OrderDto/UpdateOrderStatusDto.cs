using System.ComponentModel.DataAnnotations;
using VendorHub.Models;

namespace VendorHub.DTOs.OrderDto
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public OrderStatus Status { get; set; }
    }
}
