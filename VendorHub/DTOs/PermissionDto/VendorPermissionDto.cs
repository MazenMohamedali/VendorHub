namespace VendorHub.DTOs.PermissionDto
{
    public class VendorPermissionDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string? PermissionDescription { get; set; }
        public bool IsEnabled { get; set; }
    }
}
