using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorHub.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public NotificationType Type { get; set; } 
        public bool IsRead { get; set; } = false;
        [NotMapped]
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? OrderId { get; set; }
        public int? ProductId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
