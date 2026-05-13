using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VendorHub.DTOs.PermissionDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IGeneralRepository<Permission> _permissionRepository;
        private readonly IGeneralRepository<VendorPermission> _vendorPermissionRepository;
        private readonly IGeneralRepository<Vendor> _vendorRepository;

        public PermissionService(
            IGeneralRepository<Permission> permissionRepository,
            IGeneralRepository<VendorPermission> vendorPermissionRepository,
            IGeneralRepository<Vendor> vendorRepository)
        {
            _permissionRepository = permissionRepository;
            _vendorPermissionRepository = vendorPermissionRepository;
            _vendorRepository = vendorRepository;
        }

        #region Read Operations
        public async Task<GeneralResponse<IEnumerable<PermissionDto>>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository
                .GetAll()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Type)
                .Select(p => new { p.Id, p.Type, p.Description, p.Category, p.IsActive })
                .ToListAsync();

            var dtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Type.ToString(),
                Description = p.Description,
                Category = p.Category,
                IsActive = p.IsActive
            });

            return new GeneralResponse<IEnumerable<PermissionDto>>().Succeeded(dtos);
        }

        public async Task<GeneralResponse<IEnumerable<VendorPermissionDto>>> GetVendorPermissionsAsync(int vendorId)
        {
            var exists = await _vendorRepository
                .GetAll()
                .AnyAsync(v => v.Id == vendorId);

            if (!exists) return new GeneralResponse<IEnumerable<VendorPermissionDto>>().Failed("Vendor not found");

            var permissions = await _vendorPermissionRepository
                 .GetAll()
                 .Where(vp => vp.VendorId == vendorId)
                 .Include(vp => vp.Permission)
                 .OrderBy(vp => vp.Permission.Category)
                 .ThenBy(vp => vp.Permission.Type)
                 .Select(vp => new {
                     vp.Id,
                     vp.VendorId,
                     vp.PermissionId,
                     vp.Permission.Type,
                     vp.Permission.Description,
                     vp.IsEnabled
                 })
                 .ToListAsync();

            var dtos = permissions.Select(p => new VendorPermissionDto
            {
                Id = p.Id,
                VendorId = p.VendorId,
                PermissionId = p.PermissionId,
                PermissionName = p.Type.ToString(),
                PermissionDescription = p.Description,
                IsEnabled = p.IsEnabled
            });

            return new GeneralResponse<IEnumerable<VendorPermissionDto>>().Succeeded(dtos);
        }
        #endregion

        #region Write Operations
        public async Task<GeneralResponse> CreatePermissionAsync(CreatePermissionDto dto)
        {
            if (await _permissionRepository.GetAll().AnyAsync(p => p.Type == dto.Type))
                return new GeneralResponse().Failed("Permission already exists");

            var permission = new Permission
            {
                Type = dto.Type,
                Description = dto.Description,
                Category = dto.Category,
            };

            await _permissionRepository.AddAsync(permission);
            await _permissionRepository.SaveAsync();
            return new GeneralResponse().Succeeded("Permission created.");
        }

        public async Task<GeneralResponse> EnablePermissionForVendorAsync(int vendorId, PermissionType type)
        {
            return await SetVendorPermissionAsync(vendorId, type, true);
        }

        public async Task<GeneralResponse> DisablePermissionForVendorAsync(int vendorId, PermissionType type)
        {
            return await SetVendorPermissionAsync(vendorId, type, false);
        }

        public async Task<GeneralResponse> EnablePermissionForVendorAsync(PermissionType type)
        {
            return await SetPermissionForRoleAsync(type, true);
        }

        public async Task<GeneralResponse> DisablePermissionForVendorAsync(PermissionType type)
        {
            return await SetPermissionForRoleAsync(type, false);
        }


        public async Task<bool> HasPermissionAsync(int vendorId, PermissionType type)
        {
            return await _vendorPermissionRepository
                .GetAll()
                .AnyAsync(vp =>
                    vp.VendorId == vendorId &&
                    vp.Permission.Type == type &&
                    vp.IsEnabled);
        }
        #endregion

        #region helpers
        private async Task<GeneralResponse> SetPermissionForRoleAsync(PermissionType type, bool enable)
        {
            var permission = await _permissionRepository.GetAll().FirstOrDefaultAsync(p => p.Type == type);
            if (permission == null) return new GeneralResponse().Failed("Permission not found");

            var vendors = await _vendorRepository.GetAll().ToListAsync();

            foreach (var vendor in vendors)
            {
                await UpsertVendorPermission(vendor.Id, permission.Id, enable);
            }

            await _vendorPermissionRepository.SaveAsync();
            return new GeneralResponse().Succeeded($"Permission updated for all vendors.");
        }

        private async Task<GeneralResponse> SetVendorPermissionAsync(int vendorId, PermissionType type, bool enable)
        {
            var validation = await ValidateVendorWithPermission(vendorId, type);
            if (validation.Error != null) return new GeneralResponse().Failed(validation.Error);

            await UpsertVendorPermission(validation.Vendor!.Id, validation.Permission!.Id, enable);
            await _vendorPermissionRepository.SaveAsync();

            return new GeneralResponse().Succeeded($"Permission {(enable ? "enabled" : "disabled")}.");
        }

        private async Task UpsertVendorPermission(int vendorId, int permissionId, bool enable)
        {
            var existing = await _vendorPermissionRepository
                .GetAll()
                .FirstOrDefaultAsync(vp => vp.VendorId == vendorId && vp.PermissionId == permissionId);

            if (existing != null)
            {
                existing.IsEnabled = enable;
                existing.UpdatedAt = DateTime.UtcNow;
                await _vendorPermissionRepository.UpdateAsync(existing);
                return;
            }

            if (enable)
            {
                await _vendorPermissionRepository.AddAsync(new VendorPermission
                {
                    VendorId = vendorId,
                    PermissionId = permissionId,
                    IsEnabled = true
                });
            }
        }

        private record ValidationResult(string? Error, Vendor? Vendor, Permission? Permission);
        private async Task<ValidationResult> ValidateVendorWithPermission(int vendorId, PermissionType type)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return new ValidationResult("Vendor not found", null, null);

            var permission = await _permissionRepository.GetAll()
                .FirstOrDefaultAsync(p => p.Type == type && p.IsActive);

            if (permission == null) return new ValidationResult("Permission not found or inactive", null, null);

            return new ValidationResult(null, vendor, permission);
        }
        #endregion

        #region Mapping Expressions
        private static Expression<Func<Permission, PermissionDto>> PermissionToDto() =>
            p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                IsActive = p.IsActive
            };

        private static Expression<Func<VendorPermission, VendorPermissionDto>> VendorPermissionToDto() =>
            vp => new VendorPermissionDto
            {
                Id = vp.Id,
                VendorId = vp.VendorId,
                PermissionId = vp.PermissionId,
                PermissionName = vp.Permission.Name,
                PermissionDescription = vp.Permission.Description,
                IsEnabled = vp.IsEnabled
            };
    }
    #endregion
}