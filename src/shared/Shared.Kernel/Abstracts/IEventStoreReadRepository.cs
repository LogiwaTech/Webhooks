using Shared.Kernel.Abstracts;
using Shared.Kernel.EventStore;

namespace Shared.Kernel.Absract;

public interface IEventStoreReadRepository<T> where T : IEventData 
{
    Task<List<EventDataWrapper<T>>> ReadFromStreamBackwards(long maxCount = 9223372036854775807L, long startPosition = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken());
    Task<List<EventDataWrapper<T>>> ReadFromStreamForwards(long maxCount = 9223372036854775807L, long startPosition = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken());
}