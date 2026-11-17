// In-Memory Message Broker (Simulates Apache Kafka Topic)
using System.Collections.Concurrent;

public class InMemoryKafkaBroker : IMessageBroker
{
    private readonly ConcurrentQueue<EnterpriseNotificationEvent> _queue = new();

    public Task PublishAsync(EnterpriseNotificationEvent notificationEvent)
    {
        _queue.Enqueue(notificationEvent);
        Console.WriteLine($"[KAFKA BROKER] Event '{notificationEvent.EventId}' published to topic 'enterprise-notifications'.");
        return Task.CompletedTask;
    }

    public Task<EnterpriseNotificationEvent?> ConsumeAsync(CancellationToken cancellationToken)
    {
        if (_queue.TryDequeue(out var notificationEvent))
        {
            return Task.FromResult<EnterpriseNotificationEvent?>(notificationEvent);
        }
        return Task.FromResult<EnterpriseNotificationEvent?>(null);
    }
}