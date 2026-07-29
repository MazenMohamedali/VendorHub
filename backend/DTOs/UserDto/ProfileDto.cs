namespace VendorHub.DTOs.UserDto
{
    namespace VendorHub.DTOs.UserDto
    {
        public class BaseProfileDto
        {
            public int Id { get; init; }
            public string Email { get; init; } = string.Empty;
            public string FirstName { get; init; } = string.Empty;
            public string SecondName { get; init; } = string.Empty;
            public string PhoneNumber { get; init; } = string.Empty;
            public string Role { get; init; } = string.Empty;
            public string AccountStatus { get; init; } = string.Empty;
        }
    }
}
