using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;

namespace VendorHub.Services
{
    public interface IPermissionService
    {
        #region Read Operations
        Task<GeneralResponse<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<IEnumerable<VendorPermissionDto>>> GetVendorPermissionsAsync(
            int vendorId,
            CancellationToken cancellationToken = default);

        Task<bool> HasPermissionAsync(
            int vendorId,
            PermissionType type,
            CancellationToken cancellationToken = default);
        #endregion

        #region Write Operations
        Task<GeneralResponse> EnablePermissionForVendorAsync(
            int vendorId,
            PermissionType type,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> DisablePermissionForVendorAsync(
            int vendorId,
            PermissionType type,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> GlobalEnablePermissionAsync(
            PermissionType type,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse> GlobalDisablePermissionAsync(
            PermissionType type,
            CancellationToken cancellationToken = default);
        #endregion

        Task AssignDefaultVendorPermissionsAsync(int vendorId, CancellationToken cancellationToken = default);
    }
}
