
using System.Threading.Channels;

namespace VendorHub.Events
{
    public interface IEventQueue<TEvent>
    {
        IAsyncEnumerable<TEvent> DequeueAllAsync(CancellationToken cancellationToken);
        ValueTask EnqueueAsync(TEvent evnt, CancellationToken cancellationToken = default);
    }
}
