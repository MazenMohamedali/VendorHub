namespace VendorHub.DTOs.UserDto
{
    public class RegisterCustomerDto : RegisterUserDto
    {
        public string? Address { get; set; } = string.Empty;
    }
}
