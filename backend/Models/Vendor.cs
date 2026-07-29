namespace VendorHub.Models
{
    public class Vendor : User
    {
        public string StoreName { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0.0m;
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public PermissionType Permission { get; set; } = PermissionType.None;
    }
}
