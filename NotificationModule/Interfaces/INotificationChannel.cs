using System.ComponentModel.DataAnnotations;

public interface INotificationChannel
{
    string ChannelType { get; }
    Task<bool> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    Task<bool> SendBatchAsync(IEnumerable<NotificationMessage> messages, CancellationToken cancellationToken = default);
}
