using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IPermissionService
    {
        Task<GeneralResponse> CreatePermissionAsync(CreatePermissionDto dto);
        Task<GeneralResponse> DisablePermissionForVendorAsync(int vendorId, PermissionType type);
        Task<GeneralResponse> DisablePermissionForVendorAsync(PermissionType type);
        Task<GeneralResponse> EnablePermissionForVendorAsync(int vendorId, PermissionType type);
        Task<GeneralResponse> EnablePermissionForVendorAsync(PermissionType type);
        Task<GeneralResponse<IEnumerable<PermissionDto>>> GetAllPermissionsAsync();
        Task<GeneralResponse<IEnumerable<VendorPermissionDto>>> GetVendorPermissionsAsync(int vendorId);
        Task<bool> HasPermissionAsync(int vendorId, PermissionType type);
    }
}