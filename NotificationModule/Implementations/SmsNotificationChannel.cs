// Concrete Channels
public class SmsNotificationChannel : INotificationChannel
{
    public string ChannelType => "SMS";

    public async Task<bool> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"   [SMS PROVIDER] Individual Send -> Sent to {message.Address} | Content: {message.Body}");
        await Task.Delay(20, cancellationToken);
        return true;
    }

    public async Task<bool> SendBatchAsync(IEnumerable<NotificationMessage> messages, CancellationToken cancellationToken = default)
    {
        var list = messages.ToList();
        Console.WriteLine($"   [SMS PROVIDER] Bulk Batch Send -> Dispatched {list.Count} messages in 1 single HTTP request to SendGrid API.");
        await Task.Delay(50, cancellationToken);
        return true;
    }
}
