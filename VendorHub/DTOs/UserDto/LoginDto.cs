using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Remote(action: "IsUniqueEmail", controller: "Account", ErrorMessage = "Email already exists")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
