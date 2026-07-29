namespace VendorHub.Events
{
    public interface ICustomEventHandler<TEvent>
    {
        Task HandleAsync(TEvent evnt);
    }
}
