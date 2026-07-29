using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorHub.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImgUrl { get; set; }
        public long Quantity { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.PENDING;
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public long ViewersNo { get; set; } = 0;
        public long OverallStars { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[8];

        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
