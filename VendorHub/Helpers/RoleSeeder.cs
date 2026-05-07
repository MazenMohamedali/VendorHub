using Microsoft.AspNetCore.Identity;

namespace VendorHub.Helpers
{
    public class RoleSeeder
    {
        public static string[] Roles => new[] { "Admin", "Vendor", "Customer" };
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }
    }
}
