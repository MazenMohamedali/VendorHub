namespace VendorHub.Events
{
    public class EventPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        public EventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent evnt)
        {
            var handlers = _serviceProvider.GetServices<ICustomEventHandler<TEvent>>();
            foreach (var handler in handlers)
                await handler.HandleAsync(evnt);
        }

    }
}
