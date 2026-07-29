namespace VendorHub.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveNewPurchaseNotification(object notificationPayload);
        Task ReceiveOrderStatusNotification(object payload);
    }
}
