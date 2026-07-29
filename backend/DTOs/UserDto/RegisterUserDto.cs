using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "can only contain letters and spaces")]
        public string FirstName { get; set; } = string.Empty;


        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "can only contain letters and spaces")]
        public string SecondName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [Remote(action: "IsUniqueEmail", controller: "Account", ErrorMessage = "Email already exists")]
        public string Email { get; set; } = string.Empty;


        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Not a valid Egyptian phone number")]
        public string? PhoneNumber { get; set; } = string.Empty;
    }
}
