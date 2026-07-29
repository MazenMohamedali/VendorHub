using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.CustomerDto
{
    public class UpdateCustomerProfileDto
    {
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "can only contain letters and spaces")]
        public string? FirstName { get; init; }

        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "can only contain letters and spaces")]
        public string? SecondName { get; init; }

        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Not a valid Egyptian phone number")]
        public string? PhoneNumber { get; init; }

        public string? Address { get; init; }
    }
}
