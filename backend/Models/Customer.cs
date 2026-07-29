namespace VendorHub.Models
{
    public class Customer : User
    {
        public string Address { get; set; } = string.Empty;
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<Order>? Transactions { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
