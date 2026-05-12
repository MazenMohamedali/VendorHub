using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class UpdateProfileDto
    {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(100, MinimumLength = 2)]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(100, MinimumLength = 2)]
            public string SecondName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            public string PhoneNumber { get; set; }

            [StringLength(500)]
            public string? Address { get; set; }
            [StringLength(200, MinimumLength = 3)]
            public string? StoreName { get; set; }
    }
}
