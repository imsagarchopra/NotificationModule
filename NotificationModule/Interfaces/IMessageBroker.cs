// Simulates the Distributed Message Queue (Kafka / RabbitMQ)
public interface IMessageBroker
{
    Task PublishAsync(EnterpriseNotificationEvent notificationEvent);
    Task<EnterpriseNotificationEvent?> ConsumeAsync(CancellationToken cancellationToken);
}