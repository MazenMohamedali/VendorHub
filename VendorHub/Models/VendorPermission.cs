using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorHub.Models
{
    public class VendorPermission
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }


        [ForeignKey("Permission")]
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
