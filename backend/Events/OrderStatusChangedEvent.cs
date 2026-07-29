using VendorHub.Models;

namespace VendorHub.Events
{
    public record OrderStatusChangedEvent(int CustomerId, int OrderId, string? NewStatus);
}
