using System.Text;
using System.Text.Json;
using EventStore.Client; 
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts;
using Shared.Kernel.Extensions;

namespace Shared.Kernel.EventStore.Persistence;

public class EventStoreReadRepository<T> : EventStoreBaseRepository<T>, IEventStoreReadRepository<T> where T : IEventData
{
    private readonly IConfiguration _configuration;

    private static string StreamName => EventStoreExtensions.GenerateStreamName<T>();

    public EventStoreReadRepository(IConfiguration configuration) : base(configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<EventDataWrapper<T>>> ReadFromStreamBackwards(long maxCount = 9223372036854775807L, long startPosition = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken())
    {
        var events = await ReadEventDataFromStream(
            maxCount,
            startPosition,
            Direction.Backwards,
            resolveLinkTos,
            token: token);

        return events;
    }


    public async Task<List<EventDataWrapper<T>>> ReadFromStreamForwards(long maxCount = 9223372036854775807L, long startPosition = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken())
    {
        var events = await ReadEventDataFromStream(
            maxCount,
            startPosition,
            Direction.Forwards,
            resolveLinkTos,
            token: token);

        return events;
    }

    private EventStoreClient.ReadStreamResult ReadFromStream(long maxCount = 9223372036854775807L, long startPosition = 0, Direction direction = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken())
    {
        var revision = StreamPosition.FromInt64(startPosition);

        var currentStreamName = StreamName;
        var version = _configuration.GetValue<string>("version");
        if (!string.IsNullOrEmpty(version))
        {
            currentStreamName += $"-{version}";
        }

        var events = _eventStoreClient.ReadStreamAsync(
            direction: direction,
            streamName: currentStreamName,
            revision: revision,
            maxCount: maxCount,
            resolveLinkTos: resolveLinkTos,
            cancellationToken: token);

        if (events.ReadState.Result == ReadState.StreamNotFound)
        {
            throw new Exception("Stream not found");
        }

        return events;
    }

    private async Task<List<EventDataWrapper<T>>> ReadEventDataFromStream(long maxCount = 9223372036854775807L, long startPosition = 0, Direction direction = 0, bool resolveLinkTos = false, CancellationToken token = new CancellationToken())
    {
        var events = ReadFromStream(
                maxCount,
                startPosition,
                direction,
                resolveLinkTos,
                token
            );

        var streamData = new List<EventDataWrapper<T>>();

        if (streamData == null) throw new ArgumentNullException(nameof(streamData));

        await foreach (var @event in events)
        {
            var eventFromStream = @event.Event;
            var jsonString = Encoding.UTF8.GetString(eventFromStream.Data.ToArray());
            var storeEvent = JsonSerializer.Deserialize<T>(jsonString);

            if (storeEvent == null) continue;

            var eventData = new EventDataWrapper<T>(
                eventFromStream.Created,
                eventFromStream.Position.CommitPosition,
                eventFromStream.Position.PreparePosition,
                eventFromStream.ContentType,
                eventFromStream.EventId,
                storeEvent,
                eventFromStream.EventNumber.ToString(),
                eventFromStream.EventType,
                eventFromStream.EventStreamId);

            streamData.Add(eventData);
        }

        return streamData;
    }
}