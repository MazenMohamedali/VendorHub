using System.ComponentModel.DataAnnotations;

namespace VendorHub.DTOs.UserDto
{
    public class UpdateStoreNameDto
    {
        [Required(ErrorMessage = "Store name is required")]
        [StringLength(200, MinimumLength = 3)]
        public string StoreName { get; set; }
    }
}
