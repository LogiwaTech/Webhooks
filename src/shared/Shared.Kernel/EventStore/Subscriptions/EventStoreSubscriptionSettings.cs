namespace Shared.Kernel.EventStore.Subscriptions;

public class PersistentSubscription
{
    public bool ResolveLinkTos { get; set; }
    public int StartFrom { get; set; }
    public bool ExtraStatistics { get; set; }
    public int MessageTimeoutInSeconds { get; set; }
    public int MaxRetryCount { get; set; }
    public int LiveBufferSize { get; set; }
    public int ReadBatchSize { get; set; }
    public int HistoryBufferSize { get; set; }
    public int CheckPointAfterInSeconds { get; set; }
    public int MinCheckPointCount { get; set; }
    public int MaxCheckPointCount { get; set; }
    public int MaxSubscriberCount { get; set; }
    public string NamedConsumerStrategy { get; set; }
}

public class EventStoreSubscriptionSettings
{
    public string UserName { get; set; }
    public string Password { get; set; }
    
    public string ConnectionString { get; set; }
    public PersistentSubscription PersistentSubscription { get; set; }
}