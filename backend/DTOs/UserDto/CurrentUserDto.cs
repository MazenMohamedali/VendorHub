namespace VendorHub.DTOs.UserDto
{
    public class CurrentUserDto
    {
        public int Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}
