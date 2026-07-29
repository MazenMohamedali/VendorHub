using VendorHub.DTOs.UserDto.VendorHub.DTOs.UserDto;

namespace VendorHub.DTOs.VendorDto
{
    public class VendorProfileDto : BaseProfileDto
    {
        public string StoreName { get; init; } = string.Empty;
        public decimal Balance { get; init; }
    }
}
