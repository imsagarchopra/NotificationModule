public interface INotificationOrchestrator
{
    Task DispatchAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default);
}