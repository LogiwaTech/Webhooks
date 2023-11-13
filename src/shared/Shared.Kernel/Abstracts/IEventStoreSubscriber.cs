using Shared.Kernel.Abstracts;
using Shared.Kernel.EventStore.Subscriptions;

namespace Shared.Kernel.Absract;

public interface IEventStoreSubscriber<TEvent> where TEvent : IEventData
{
    Task SubscribeToPersistentStreamAsync(
        SubscriptionBaseHandlers<TEvent> subscriptionHandler,
        CancellationToken cancellationToken = default (CancellationToken));
}