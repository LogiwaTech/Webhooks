using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts; 
using ResolvedEvent = EventStore.Client.ResolvedEvent;

namespace Shared.Kernel.EventStore.Subscriptions;

public abstract class SubscriptionBaseHandlers<TEvent> : BackgroundService
    where TEvent : IEventData
{
    private readonly ILogger<SubscriptionBaseHandlers<TEvent>> _logger;

    private readonly IServiceProvider _serviceProvider;

    protected SubscriptionBaseHandlers(ILogger<SubscriptionBaseHandlers<TEvent>> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected abstract Task<SubscriptionDroppedReason> SubscriptionDropped(
        global::EventStore.Client.PersistentSubscription subscriptionBase, SubscriptionDroppedReason reason);

    protected abstract Task<ResolvedEvent> EventAppeared(ResolvedEvent resolvedEvent);

    public async Task TriggerAsync(global::EventStore.Client.PersistentSubscription subscription, ResolvedEvent resolvedEvent, bool shouldAcknowledge = true)
    {
        try
        {
            await this.EventAppeared(resolvedEvent);

            if (shouldAcknowledge)
                await subscription.Ack(resolvedEvent);
        }
        catch (Exception e)
        {
            if (CheckIfExceptionMessageIsWarning(e.Message) && shouldAcknowledge)
            {
                _logger.LogWarning(e, $"Could not perform event appeared: {e.Message}");
                await subscription.Ack(resolvedEvent);
                await Task.Delay(250);
            }
            else
            {
                _logger.LogCritical(e, $"Could not perform event appeared: {e.Message}");
                await subscription.Nack(global::EventStore.Client.PersistentSubscriptionNakEventAction.Park, e.Message,
                    new[] { resolvedEvent });
            }
        }
    }

    public async Task OnSubscriptionDroppedAsync(global::EventStore.Client.PersistentSubscription subscription, SubscriptionDroppedReason reason)
    {
        try
        {
            await this.SubscriptionDropped(subscription, reason);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"Could not perform subscription dropped: {e.Message}");
        }
    }

    public async Task ConnectToPublisherAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var eventStoreSubscriber =
            scope.ServiceProvider.GetRequiredService<IEventStoreSubscriber<TEvent>>();

        var cancellationToken = new CancellationToken();
        await eventStoreSubscriber.SubscribeToPersistentStreamAsync(this, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service running.");

        await ConnectToPublisherAsync();
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }


    public bool CheckIfExceptionMessageIsWarning(string message)
    {
        List<string> warningErrorList = new()
        {
            "There is no subscription for this customer",
            "Can't write the message because the previous write is in progress."
        };

        if (warningErrorList.Contains(message))
        {
            return true;
        }

        return false;
    }
}