namespace VendorHub.Events
{
    public class EventConsumerBackgroundService<TEvent> : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventQueue<TEvent> _queue;
        private readonly ILogger<EventConsumerBackgroundService<TEvent>> _logger;

        public EventConsumerBackgroundService(
            IServiceProvider serviceProvider,
            IEventQueue<TEvent> queue,
            ILogger<EventConsumerBackgroundService<TEvent>> logger)
        {
            _serviceProvider = serviceProvider;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background worker for processing {EventType} started.", typeof(TEvent).Name);

            var eventStream = _queue.DequeueAllAsync(cancellationToken);
            await foreach (var evnt in eventStream.WithCancellation(cancellationToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<EventPublisher>();
                    await publisher.PublishAsync(evnt);
                } catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to process an isolated instance of event type {EventType}.", typeof(TEvent).Name);
                }
            }
            _logger.LogInformation("Background worker for processing {EventType} stopped gracefully.", typeof(TEvent).Name);
        }
    }
}
