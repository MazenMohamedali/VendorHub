using VendorHub.DTOs.CustomerDto;
using VendorHub.DTOs.sharedDto;

namespace VendorHub.Services
{
    public interface ICustomerService
    {
        Task<GeneralResponse<CustomerProfileDto>> GetCustomerProfileAsync(int userId, CancellationToken cancellationToken = default);
        Task<GeneralResponse<CustomerProfileDto>> UpdateCustomerProfileAsync(int userId, UpdateCustomerProfileDto dto, CancellationToken cancellationToken = default);
    }
}
