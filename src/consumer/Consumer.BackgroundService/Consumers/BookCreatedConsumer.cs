using EventStore.Client;
using Microsoft.OpenApi.Extensions;
using Publisher.Service.Commands.BookCreated;
using Shared.Kernel.EventStore.Subscriptions;
using Shared.Kernel.Extensions;

namespace Consumer.BackgroundService.Consumers
{
    public class BookCreatedConsumer : SubscriptionBaseHandlers<BookCreatedCommand>
    {
        private readonly ILogger<BookCreatedConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BookCreatedConsumer(IServiceProvider serviceProvider, ILogger<BookCreatedConsumer> logger) : base(logger, serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task<ResolvedEvent> EventAppeared(ResolvedEvent resolvedEvent)
        {
            var eventJson = resolvedEvent.Event.Data.ToJsonString();
            _logger.LogInformation(eventJson);
            return resolvedEvent;
        }

        protected override async Task<SubscriptionDroppedReason> SubscriptionDropped(EventStore.Client.PersistentSubscription subscriptionBase, SubscriptionDroppedReason reason)
        {
            _logger.LogCritical($"BookCreatedConsumer subscription dropped {reason.GetDisplayName()}");
            Console.WriteLine($"BookCreatedConsumer subscription dropped {reason.GetDisplayName()}");
            await Task.Delay(100);
            return reason;
        }
    }
}
