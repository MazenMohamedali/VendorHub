using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.VendorDto
{
    public class UpdateVendorProfileDto
    {
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        public string? FirstName { get; init; }

        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Second name can only contain letters and spaces")]
        public string? SecondName { get; init; }

        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Not a valid Egyptian phone number")]
        public string? PhoneNumber { get; init; }

        [StringLength(150, MinimumLength = 3)]
        public string? StoreName { get; init; }
    }
}
