using VendorHub.Models;

namespace VendorHub.DTOs.UserDto
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public List<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
