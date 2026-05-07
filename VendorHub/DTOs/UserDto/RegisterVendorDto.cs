using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class RegisterVendorDto : RegisterUserDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string StoreName { get; set; } = string.Empty;
    }
}
