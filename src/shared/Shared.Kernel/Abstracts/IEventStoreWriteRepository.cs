
using Shared.Kernel.Abstracts;
using Shared.Kernel.EventStore;

namespace Shared.Kernel.Absract;

public interface IEventStoreWriteRepository<T> where T : IEventData
{
    Task<AppendEventToStreamResponse> AppendToStream(
        T eventData,
        int streamState = 1,
        string contentType = "application/json",
        CancellationToken cancellationToken = new CancellationToken());
}