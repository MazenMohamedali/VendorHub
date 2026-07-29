using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VendorHub.Models
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVE;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
