using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Absract;
using Shared.Kernel.Extensions; 
using Microsoft.Extensions.Logging;
using PersistentSubscriptionSettings = EventStore.Client.PersistentSubscriptionSettings;
using StreamPosition = EventStore.Client.StreamPosition;
using UserCredentials = EventStore.Client.UserCredentials;
using System.ServiceModel.Channels;
using Shared.Kernel.Abstracts;

namespace Shared.Kernel.EventStore.Subscriptions;

public class EventStoreSubscriber<TEvent> : IEventStoreSubscriber<TEvent>
    where TEvent : IEventData
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<EventStoreSubscriber<TEvent>> _logger;

    private static string StreamName => EventStoreExtensions.GenerateStreamName<TEvent>();

    private static string GroupName => EventStoreExtensions.GenerateSubscriptionGroupName<TEvent>();

    private EventStoreSubscriptionSettings SubscriptionSettings => ReadConfiguration();

    public EventStoreSubscriber(
        IConfiguration configuration, ILogger<EventStoreSubscriber<TEvent>> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SubscribeToPersistentStreamAsync(
        SubscriptionBaseHandlers<TEvent> subscriptionHandler,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var settings = this.GeneratePersistentSubscriptionSettings();

        var credentials = new UserCredentials(SubscriptionSettings.UserName, SubscriptionSettings.Password);
        var conn = this.ConnectAsync();

        var currentStreamName = StreamName;
        var version = _configuration.GetValue<string>("version");
        if (!string.IsNullOrEmpty(version))
        {
            currentStreamName += $"-{version}";
        }

        try
        {
            await conn.CreateAsync(currentStreamName, GroupName, settings, credentials);
        }
        catch (InvalidOperationException e)
        {
            if (e.Message.Contains($"Subscription group {GroupName}") && e.Message.Contains("exists"))
            {
                //_logger.LogWarning(e, $"{e.Message}");
            }
            else
            {
                //_logger.LogError(e, e.Message);
            }

            if (!e.Message.Contains($"Subscription group {GroupName} on stream"))
                throw;
        }

        var subscription = await conn.SubscribeAsync(
            currentStreamName,
            GroupName, async (subscription, evnt, retryCount, cancellationToken) =>
            {
                await Task.Delay(500);
                await Task.FromResult(subscriptionHandler.TriggerAsync(subscription, evnt));
            },
            async (sub, reason, exception)
                =>
            {
                if (exception != null)
                    _logger.LogCritical(exception,
                        $"Event Appeared for stream {currentStreamName}. {exception.Message}" + $"Could not process ");

                await Task.FromResult(subscriptionHandler.OnSubscriptionDroppedAsync(sub, reason));

                if (reason != SubscriptionDroppedReason.Disposed)
                {
                    await ReSubscribe(subscriptionHandler);
                }
            }
        );

    }

    private async Task ReSubscribe(SubscriptionBaseHandlers<TEvent> subscriptionHandler)
    {
        int maxResubscriptionAttemptCount = 3;

        for (int attempCount = 0; attempCount < maxResubscriptionAttemptCount; attempCount++)
        {
            try
            {
                if (attempCount > 0)
                {
                    await Task.Delay(1000);
                }

                await subscriptionHandler.ConnectToPublisherAsync();
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Could not perform resubscription: {exception.Message}. Try count: {attempCount}");
            }
        }
    }

    private EventStoreSubscriptionSettings ReadConfiguration()
    {
        var settings = _configuration.GetSection("EventStore").Get<EventStoreSubscriptionSettings>();

        if (settings == null)
            throw new Exception("Please add EventStoreDB user credentials to your appsettings.json");

        return settings;
    }

    private PersistentSubscriptionSettings GeneratePersistentSubscriptionSettings()
    {
        var settings = _configuration.GetSection("EventStore").Get<EventStoreSubscriptionSettings>();

        if (settings == null)
            throw new ApplicationException(
                "EventStore:PersistentSubscription configurations could not be loaded. Please check your appsettins.json");


        var startPosition = StreamPosition.Start;
        var resolveLinkTos = SubscriptionSettings.PersistentSubscription.ResolveLinkTos;
        var extraStatistics = SubscriptionSettings.PersistentSubscription.ExtraStatistics;
        var maxRetry = 500;
        var liveBufferSize = 500;
        var readBatchSize = 30;
        var historyBufferSize = 20;
        var minCheckPointCount = 10;
        var maxCheckPointCount = 1000;
        var maxSubscriberCount = 0;
        var namedConsumerStrategy = "RoundRobin";

        TimeSpan? messageTimeout = null;
        TimeSpan? checkPointAfterInSeconds = null;


        if (SubscriptionSettings.PersistentSubscription.MessageTimeoutInSeconds > 0)
            messageTimeout = TimeSpan.FromSeconds(SubscriptionSettings.PersistentSubscription.MessageTimeoutInSeconds);

        if (SubscriptionSettings.PersistentSubscription.StartFrom > 0)
            startPosition = StreamPosition.FromInt64(SubscriptionSettings.PersistentSubscription.StartFrom);

        if (SubscriptionSettings.PersistentSubscription.MaxRetryCount > 0)
            maxRetry = SubscriptionSettings.PersistentSubscription.MaxRetryCount;

        if (SubscriptionSettings.PersistentSubscription.LiveBufferSize > 0)
            liveBufferSize = SubscriptionSettings.PersistentSubscription.LiveBufferSize;

        if (SubscriptionSettings.PersistentSubscription.ReadBatchSize > 0)
            readBatchSize = SubscriptionSettings.PersistentSubscription.ReadBatchSize;

        if (SubscriptionSettings.PersistentSubscription.HistoryBufferSize > 0)
            historyBufferSize = SubscriptionSettings.PersistentSubscription.HistoryBufferSize;

        if (SubscriptionSettings.PersistentSubscription.CheckPointAfterInSeconds > 0)
            checkPointAfterInSeconds = TimeSpan.FromSeconds(SubscriptionSettings.PersistentSubscription.CheckPointAfterInSeconds);

        if (SubscriptionSettings.PersistentSubscription.MinCheckPointCount > 0)
            minCheckPointCount = SubscriptionSettings.PersistentSubscription.MinCheckPointCount;

        if (SubscriptionSettings.PersistentSubscription.MaxCheckPointCount > 0)
            maxCheckPointCount = SubscriptionSettings.PersistentSubscription.MaxCheckPointCount;

        if (SubscriptionSettings.PersistentSubscription.MaxSubscriberCount > 0)
            maxSubscriberCount = SubscriptionSettings.PersistentSubscription.MaxSubscriberCount;

        if (!string.IsNullOrEmpty(SubscriptionSettings.PersistentSubscription.NamedConsumerStrategy))
            namedConsumerStrategy = SubscriptionSettings.PersistentSubscription.NamedConsumerStrategy;

        var perSettings = new PersistentSubscriptionSettings(
                resolveLinkTos,
                startPosition,
                extraStatistics,
                messageTimeout,
                maxRetry,
                liveBufferSize,
                readBatchSize,
                historyBufferSize,
                checkPointAfterInSeconds,
                minCheckPointCount,
                maxCheckPointCount,
                maxSubscriberCount,
                namedConsumerStrategy
            );

        return perSettings;
    }

    private EventStorePersistentSubscriptionsClient ConnectAsync()
    {
        var connectionString = SubscriptionSettings.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception(
                "Please specify EventStoreDB connection string in appsettings.json's EventStore:ConnectionString section.");

        var client = new EventStorePersistentSubscriptionsClient(
            EventStoreClientSettings.Create(connectionString)
        );

        // var connection = EventStoreConnection.Create(
        //     new Uri(connectionString)
        // );
        //
        // await connection.ConnectAsync();

        return client;
    }
}