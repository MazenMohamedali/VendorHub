using System.ComponentModel.DataAnnotations;
using VendorHub.Models;

namespace VendorHub.DTOs.PermissionDto
{
    public class CreatePermissionDto
    {
        public string RoleName { get; set; } = string.Empty;
        public PermissionType CombinedPermissions { get; set; }
    }
}
