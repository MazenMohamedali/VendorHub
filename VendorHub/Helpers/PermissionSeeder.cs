//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using VendorHub.Models;
//using VendorHub.Repository;

//namespace VendorHub.Helpers
//{
//    public class PermissionSeeder
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            var permissionRepository = serviceProvider.GetRequiredService<IGeneralRepository<Permission>>();

//            var persissions = new List<(PermissionType Type, string Description, string Category)>
//            {
//                (PermissionType.CanUploadProducts, "Allow vendor to upload products", "Product"),
//                (PermissionType.CanEditProducts, "Allow vendor to edit products", "Product"),
//                (PermissionType.CanDeleteProducts, "Allow vendor to delete products", "Product"),
//                (PermissionType.CanViewProducts, "Allow vendor to view products", "Product"),


//                (PermissionType.CanViewOrders, "Allow vendor to view orders", "Order"),
//                (PermissionType.CanUpdateOrderStatus, "Allow vendor to update order status", "Order"),
//                (PermissionType.CanCancelOrders, "Allow vendor to cancel orders", "Order"),


//                (PermissionType.CanViewAnalytics, "Allow vendor to view analytics", "Account"),


//                (PermissionType.CanManageInventory, "Allow vendor to manage inventory", "Product")
//            };

//            var existing = await permissionRepository
//                .GetAll()
//                .ToListAsync();

//            foreach (var (type, description, category) in persissions)
//            {
//                var exists = existing.Any(p => p.Type == type);
//                if (exists) return;
//                var newPermission = new Permission
//                {
//                    Type = type,
//                    Description = description,
//                    Category = category
//                };
//            }

//            await permissionRepository.SaveAsync();
//        }
//    }
//}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Helpers
{
    public class PermissionSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var permissionRepository = serviceProvider.GetRequiredService<IGeneralRepository<Permission>>();

            var permissions = new List<(PermissionType Type, string Description, string Category)>
            {
                (PermissionType.CanUploadProducts, "Allow vendor to upload products", "Product"),
                (PermissionType.CanEditProducts, "Allow vendor to edit products", "Product"),
                (PermissionType.CanDeleteProducts, "Allow vendor to delete products", "Product"),
                (PermissionType.CanViewProducts, "Allow vendor to view products", "Product"),
                (PermissionType.CanViewOrders, "Allow vendor to view orders", "Order"),
                (PermissionType.CanUpdateOrderStatus, "Allow vendor to update order status", "Order"),
                (PermissionType.CanCancelOrders, "Allow vendor to cancel orders", "Order"),
                (PermissionType.CanViewAnalytics, "Allow vendor to view analytics", "Account"),
                (PermissionType.CanManageInventory, "Allow vendor to manage inventory", "Product")
            };

            var existingTypes = await permissionRepository
                .GetAll()
                .Select(p => p.Type)
                .ToListAsync();

            foreach (var (type, description, category) in permissions)
            {
                if (!existingTypes.Contains(type))
                {
                    var newPermission = new Permission
                    {
                        Type = type,
                        Description = description,
                        Category = category,
                        IsActive = true
                    };
                    await permissionRepository.AddAsync(newPermission);
                }
            }

            await permissionRepository.SaveAsync();
        }
    }
}