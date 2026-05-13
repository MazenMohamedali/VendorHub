using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.Vendors;

namespace VendorHub.Services
{
    public interface IVendorService
    {
        Task<GeneralResponse<IEnumerable<VendorDetailsDto>>> GetAllVendorsAsync();
    }
}