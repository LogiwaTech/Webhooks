using EventStore.Client;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts;

namespace Shared.Kernel.EventStore;

public class EventDataWrapper<T> where T : IEventData
{
    public EventDataWrapper(DateTime created, ulong commitPosition, ulong preparePosition, string contentType, Uuid eventId, T data, string eventNumber, string eventType, string eventStreamId)
    {
        Created = created;
        CommitPosition = commitPosition;
        PreparePosition = preparePosition;
        ContentType = contentType;
        EventId = eventId;
        Data = data;
        EventNumber = eventNumber;
        EventType = eventType;
        EventStreamId = eventStreamId;
    }
    
    public T Data { get; }
    
    public DateTime Created { get; }
    public ulong CommitPosition { get; }
    public ulong PreparePosition { get; }
    public string ContentType { get; }
    public Uuid EventId { get;  }
    public string EventNumber { get; }
    public string EventType { get; }
    public string EventStreamId { get; }
}