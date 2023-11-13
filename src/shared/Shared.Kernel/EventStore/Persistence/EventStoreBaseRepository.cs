using EventStore.Client; 
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts;
using Shared.Kernel.EventStore.Client;

namespace Shared.Kernel.EventStore.Persistence;

public class EventStoreBaseRepository<T>
    where T : IEventData
{
    private readonly IConfiguration _configuration;

    protected EventStoreClientConnectionSettings ClientSettings => ReadConfiguration();

    protected EventStoreClient _eventStoreClient => ConnectAsync();

    public EventStoreBaseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private EventStoreClientConnectionSettings ReadConfiguration()
    {
        var settings = _configuration.GetSection("EventStore").Get<EventStoreClientConnectionSettings>();

        if (settings == null)
            throw new Exception("Please add EventStoreDB user credentials to your appsettings.json");

        return settings;
    }
    private EventStoreClient ConnectAsync()
    {
        var connectionString = ClientSettings.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception(
                "Please specify EventStoreDB connection string in appsettings.json's EventStore:ConnectionString section.");

        var client = new EventStoreClient(EventStoreClientSettings.Create(connectionString));

        return client;
    }
}