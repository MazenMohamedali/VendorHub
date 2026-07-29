namespace VendorHub.Events
{
    public record VendorOrderSummary(
        int VendorId,
        int TotalItemsCount,
        decimal Subtotal
    );
}
