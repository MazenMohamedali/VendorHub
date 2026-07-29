namespace VendorHub.DTOs.NotificationDto
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
    }
}
