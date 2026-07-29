using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.StatisticsDto;

namespace VendorHub.Services
{
    public interface IStatisticsService
    {
        Task<GeneralResponse<VendorStatisticsDto>> GetVendorStatisticsAsync(int vendorId, CancellationToken cancellationToken = default);
    }
}
