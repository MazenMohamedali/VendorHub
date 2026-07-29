using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.VendorDto;
using VendorHub.DTOs.Vendors;

namespace VendorHub.Services
{
    public interface IVendorService
    {
        Task<GeneralResponse<PagedResult<VendorDetailsDto>>> GetAllVendorsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<GeneralResponse<VendorProfileDto>> GetVendorProfileAsync(int userId, CancellationToken cancellationToken = default);
        Task<GeneralResponse<VendorProfileDto>> UpdateVendorProfileAsync(int userId, UpdateVendorProfileDto dto, CancellationToken cancellationToken = default);
    }
}
