using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace VendorHub.Models
{
    public class VendorHubDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        #region DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        #endregion


        public VendorHubDbContext(DbContextOptions options) : base(options) { }
        public VendorHubDbContext() : base() { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Conver Enum To String
            modelBuilder.Entity<Permission>()
                .Property(p => p.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Product>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.AccountStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>(); 
            #endregion

            modelBuilder.Entity<User>()
                .HasDiscriminator<String>("Role")
                .HasValue<Admin>("Admin")
                .HasValue<Vendor>("Vendor")
                .HasValue<Customer>("Customer");

            #region decimal Precision
            modelBuilder.Entity<Product>()
                    .Property(p => p.Price)
                    .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtPurchase)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vendor>()
                .Property(v => v.Balance)
                .HasPrecision(18, 2);
            #endregion

            #region Relations
            modelBuilder.Entity<Product>()
                    .HasOne(p => p.Vendor)
                    .WithMany(v => v.Products)
                    .HasForeignKey(p => p.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Transactions)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.Favorites)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VendorPermission>()
                .HasOne(vp => vp.Vendor)
                .WithMany(v => v.Permissions)
                .HasForeignKey(vp => vp.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VendorPermission>()
                .HasOne(vp => vp.Permission)
                .WithMany(p => p.VendorPermissions)
                .HasForeignKey(vp => vp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.VendorId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.ProductId);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.CustomerId);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.CustomerId);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.ProductId);
            #endregion

            #region Constraints
            modelBuilder.Entity<Product>()
                    .HasIndex(p => new { p.VendorId, p.Name })
                    .IsUnique();

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.CustomerId, f.ProductId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.CustomerId, r.ProductId })
                .IsUnique();

            modelBuilder.Entity<VendorPermission>()
                .HasIndex(vp => new { vp.VendorId, vp.PermissionId })
                .IsUnique();
            #endregion
        }
    }
}
