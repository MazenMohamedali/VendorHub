using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class UpdateAddressDto
    {
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, MinimumLength = 5)]
        public string Address { get; set; }
    }
}
