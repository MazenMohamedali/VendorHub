using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;

namespace VendorHub.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IGeneralRepository<Vendor> _vendorRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PermissionService> _logger;
        
        public PermissionService(
            IGeneralRepository<Vendor> vendorRepository,
            ICacheService cacheService,
            ILogger<PermissionService> logger)
        {
            _vendorRepository = vendorRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        private static readonly HashSet<PermissionType> ExcludedFlags = new()
        {
            PermissionType.None,
            PermissionType.VendorAdmin,
            PermissionType.VendorStaff
        };

        private static readonly IReadOnlyList<PermissionType> CachedSystemPermissions = Enum.GetValues<PermissionType>()
            .Where(p => !ExcludedFlags.Contains(p))
            .ToList()
            .AsReadOnly();
  
        #region Read Operations
        public Task<GeneralResponse<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var result = CachedSystemPermissions
                .Select(p => new PermissionDto
                {
                    SystemName = p.ToString(),
                });

            return Task.FromResult(GeneralResponse<IEnumerable<PermissionDto>>.Succeeded(result));
        }

        public async Task<GeneralResponse<IEnumerable<VendorPermissionDto>>> GetVendorPermissionsAsync(int vendorId, CancellationToken cancellationToken = default)
        {
            var vendorPermissions = await GetVendorPermissionsInternalAsync(vendorId, cancellationToken);

            var result = CachedSystemPermissions.Select(p => new VendorPermissionDto
            {
                SystemName = p.ToString(),
                IsEnabled = (vendorPermissions & p) == p
            });

            return GeneralResponse<IEnumerable<VendorPermissionDto>>.Succeeded(result, "Vendor permissions retrieved successfully.");
        }

        public async Task<bool> HasPermissionAsync(int vendorId, PermissionType type, CancellationToken cancellationToken = default)
        {
            var vendorPermissions = await GetVendorPermissionsInternalAsync(vendorId, cancellationToken);
            return (vendorPermissions & type) == type;
        }
        #endregion

        #region Write Operations
        public async Task<GeneralResponse> EnablePermissionForVendorAsync(int vendorId, PermissionType type, CancellationToken cancellationToken = default)
        {
            return await ToggleVendorPermissionAsync(vendorId, type, enable: true, cancellationToken);
        }

        public async Task<GeneralResponse> DisablePermissionForVendorAsync(int vendorId, PermissionType type, CancellationToken cancellationToken = default)
        {
            return await ToggleVendorPermissionAsync(vendorId, type, enable: false, cancellationToken);
        }

        public async Task<GeneralResponse> GlobalEnablePermissionAsync(PermissionType type, CancellationToken cancellationToken = default)
        {
            return await ToggleGlobalPermissionAsync(type, enable: true, cancellationToken);
        }

        public async Task<GeneralResponse> GlobalDisablePermissionAsync(PermissionType type, CancellationToken cancellationToken = default)
        {
            return await ToggleGlobalPermissionAsync(type, enable: false, cancellationToken);
        }
        #endregion

        #region Private Helpers
        private async Task<PermissionType> GetVendorPermissionsInternalAsync(int vendorId, CancellationToken cancellationToken)
        {
            return await _vendorRepository
                .GetByAsNoTracking(v => v.Id == vendorId)
                .Select(v => v.Permission)
                .ToCachedFirstOrDefaultAsync(_cacheService, CacheKeys.VendorPermissions(vendorId), CacheKeys.VendorPermissions_TTL, cancellationToken);
        }

        private async Task<GeneralResponse> ToggleVendorPermissionAsync(int vendorId, PermissionType type, bool enable, CancellationToken cancellationToken)
        {
            if (ExcludedFlags.Contains(type))
            {
                _logger.LogWarningWithContext("Attempted to directly mutate restricted permission flag {PermissionType} for VendorId: {VendorId}",
                    new { PermissionType = type.ToString(), VendorId = vendorId });
                return GeneralResponse.InvalidInput($"The permission flag '{type}' cannot be altered directly.");
            }

            var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);
            if (vendor == null)
            {
                _logger.LogWarningWithContext("Permission mutation failed. VendorId: {VendorId} not found.", new { VendorId = vendorId });
                return GeneralResponse.NotFound("Target vendor specified does not exist.");
            }

            vendor.Permission = enable ? (vendor.Permission | type) : (vendor.Permission & ~type);
            vendor.UpdatedAt = DateTime.UtcNow;

            try
            {
                _vendorRepository.Update(vendor);
                await _vendorRepository.SaveAsync(cancellationToken);

                await _cacheService.RemoveAsync(CacheKeys.VendorPermissions(vendorId), cancellationToken);

                _logger.LogInfoWithContext("Vendor permission updated. VendorId: {VendorId}, Permission: {PermissionType}, Enabled: {IsEnabled}",
                    new { VendorId = vendorId, PermissionType = type.ToString(), IsEnabled = enable });

                return GeneralResponse.Succeeded($"Permission successfully {(enable ? "enabled" : "disabled")}.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogErrorWithContext("Concurrency collision modifying permissions for VendorId: {VendorId}", ex, new { VendorId = vendorId });
                return GeneralResponse.Error("Vendor permissions were modified concurrently by another process. Please refresh and try again.");
            }
        }

        private async Task<GeneralResponse> ToggleGlobalPermissionAsync(PermissionType type, bool enable, CancellationToken cancellationToken)
        {
            if (ExcludedFlags.Contains(type))
            {
                return GeneralResponse.InvalidInput($"Restricted system flag '{type}' cannot be globally assigned.");
            }

            long flagValue = (long)type;

            try
            {
                Expression<Func<Vendor, PermissionType>> permissionExpr = enable ? 
                    v => v.Permission | type
                    : v => v.Permission & ~type;

                int affectedRows = await _vendorRepository.GetAll()
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(v => v.Permission, permissionExpr)
                        .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), cancellationToken);

                _logger.LogInfoWithContext("Global permission mutated across vendors. Permission: {PermissionType}, Enabled: {IsEnabled}, AffectedVendors: {Count}",
                    new { PermissionType = type.ToString(), IsEnabled = enable, Count = affectedRows });

                return GeneralResponse.Succeeded($"Permission successfully updated across {affectedRows} vendors.");
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext("Global permission update failed for {PermissionType}", ex, new { PermissionType = type.ToString() });
                throw;
            }
        }
        #endregion
    }
}
