using System.Text;
using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts; 
using Shared.Kernel.Extensions;

namespace Shared.Kernel.EventStore.Persistence;

public class EventStoreWriteRepository<T> : EventStoreBaseRepository<T>, IEventStoreWriteRepository<T> where T : IEventData
{
    private readonly IConfiguration _configuration;

    private static string StreamName => EventStoreExtensions.GenerateStreamName<T>();

    public EventStoreWriteRepository(IConfiguration configuration) : base(configuration)
    {
        _configuration = configuration;
    }

    private StreamState StreamStateById(int state)
    {
        StreamState streamState;

        return state switch
        {
            1 => streamState = StreamState.NoStream,
            2 => streamState = StreamState.Any,
            4 => streamState = StreamState.StreamExists,
            _ => streamState = StreamState.Any
        };

        return streamState;
    }

    public async Task<AppendEventToStreamResponse> AppendToStream(T eventData, int streamState = 2, string contentType = "application/json", CancellationToken cancellationToken = new CancellationToken())
    {
        var eventDataJsonString = JsonSerializer.Serialize(eventData);

        var state = StreamStateById(streamState);

        var eventStoreData = new EventData(
            Uuid.NewUuid(),
            StreamName,
            Encoding.UTF8.GetBytes(eventDataJsonString),
            contentType: contentType
        );

        var currentStreamName = StreamName;

        var response = await _eventStoreClient.AppendToStreamAsync(
            currentStreamName,
            state,
            new List<EventData> {
                eventStoreData
            },
            cancellationToken: cancellationToken);

        return new AppendEventToStreamResponse(
            response.LogPosition.CommitPosition,
            response.LogPosition.PreparePosition);
    }
}