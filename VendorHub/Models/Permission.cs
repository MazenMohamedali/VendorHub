using System.ComponentModel.DataAnnotations;

namespace VendorHub.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        public PermissionType Type { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VendorPermission> VendorPermissions { get; set; } = new List<VendorPermission>();

        public string Name => Type.ToString();
    }
}
