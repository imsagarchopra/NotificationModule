
public interface INotificationOrchestrator
{
    Task DispatchBatchAsync(EnterpriseNotificationEvent notificationEvent, List<RecipientInfo> batchRecipients, CancellationToken cancellationToken = default);
}