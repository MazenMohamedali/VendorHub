using System.ComponentModel.DataAnnotations;
using VendorHub.Models;

namespace VendorHub.DTOs.PermissionDto
{
    public class CreatePermissionDto
    {
        [Required]
        public PermissionType Type { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }
}
