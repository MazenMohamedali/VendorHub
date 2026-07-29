using VendorHub.Models;

namespace VendorHub.Events
{
    public record OrderPlacedEvent(Order Order, List<VendorOrderSummary> VendorSummaries);
}
