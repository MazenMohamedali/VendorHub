using VendorHub.Models;

namespace VendorHub.DTOs.NotificationDto
{
    public class NotificationBaseInfoDto
    {
        public int OrderId { get; set; }
        public Product Product { get; set; }
        public OrderItem OrderItem { get; set; }
    }
}
