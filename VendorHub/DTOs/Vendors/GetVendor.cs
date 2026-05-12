namespace VendorHub.DTOs.Vendors
{
    public class VendorDetailsDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string StoreName { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        public int ProductCount { get; set; }
        public List<string> PermissionNames { get; set; } = new List<string>();
    }
}
