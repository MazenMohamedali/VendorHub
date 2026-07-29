using System.Threading.Channels;

namespace VendorHub.Events
{
    public class EventQueue<TEvent> : IEventQueue<TEvent>
    {
        public readonly Channel<TEvent> _channel;
        public EventQueue(int capacity = 1000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<TEvent>(options);
        }

        public ValueTask EnqueueAsync(TEvent evnt, CancellationToken cancellationToken = default)
        {
            if (_channel.Writer.TryWrite(evnt))
                return ValueTask.CompletedTask;

            return _channel.Writer.WriteAsync(evnt, cancellationToken); 
        }

        public IAsyncEnumerable<TEvent> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
