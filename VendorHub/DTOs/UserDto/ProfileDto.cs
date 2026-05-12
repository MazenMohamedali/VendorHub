namespace VendorHub.DTOs.UserDto
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string AccountStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Address { get; set; }
        public string? StoreName { get; set; }
        public decimal Balance { get; set; }
    }
}
