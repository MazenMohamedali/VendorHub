namespace VendorHub.DTOs.PermissionDto
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }
}
