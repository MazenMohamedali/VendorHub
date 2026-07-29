using VendorHub.Models;

namespace VendorHub.DTOs.OrderDto
{
    public record CartValidationResult
    {
        public bool IsSuccess => string.IsNullOrEmpty(Error);
        public string? Error { get; init; }
        public List<OrderItem> Items { get; init; } = new();
        public decimal TotalPrice { get; init;  }
        public static CartValidationResult Success(List<OrderItem> items, decimal totalPrice) => new() { Items = items, TotalPrice = totalPrice };
        public static CartValidationResult Failed(string error) => new() { Error = error };

    }
}
