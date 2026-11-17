public class NotificationOrchestrator : INotificationOrchestrator
{
    private readonly Dictionary<string, INotificationChannel> _channels;
    private readonly INotificationValidator _validator;
    private readonly IUserPreferenceService _preferenceService;
    private readonly ITemplateEngine _templateEngine;

    public NotificationOrchestrator(
        IEnumerable<INotificationChannel> channels,
        INotificationValidator validator,
        IUserPreferenceService preferenceService,
        ITemplateEngine templateEngine)
    {
        _channels = channels.ToDictionary(c => c.ChannelType, StringComparer.OrdinalIgnoreCase);
        _validator = validator;
        _preferenceService = preferenceService;
        _templateEngine = templateEngine;
    }

    public async Task DispatchBatchAsync(
        EnterpriseNotificationEvent notificationEvent, 
        List<RecipientInfo> batchRecipients, 
        CancellationToken cancellationToken = default)
    {
        // 1. Validation Check
        var validationResult = _validator.Validate(notificationEvent);
        if (!validationResult.IsValid) return;

        // 2. Loop through EACH requested channel in the list!
        foreach (var channelType in notificationEvent.ChannelTypes)
        {
            if (!_channels.TryGetValue(channelType, out var channel))
            {
                Console.WriteLine($"   ⚠️ Channel '{channelType}' is not registered.");
                continue;
            }

            // 3. Filter preferences INDEPENDENTLY for this specific channel
            var allowedRecipients = await _preferenceService.FilterAllowedRecipientsAsync(
                batchRecipients, 
                channelType, 
                notificationEvent.NotificationType
            );

            if (!allowedRecipients.Any())
            {
                Console.WriteLine($"   [PREFERENCE] All users opted out of {channelType} for this campaign.");
                continue;
            }

            // 4. Render Template
            var (subject, body) = await _templateEngine.RenderAsync(notificationEvent.TemplateId, notificationEvent.PayloadTokens);

            // 5. Dispatch via Channel Strategy
            if (notificationEvent.Priority == NotificationPriority.High)
            {
                foreach (var recipient in allowedRecipients)
                {
                    await channel.SendAsync(new NotificationMessage(recipient.Address, subject, body), cancellationToken);
                }
            }
            else
            {
                var messages = allowedRecipients.Select(r => new NotificationMessage(r.Address, subject, body));
                await channel.SendBatchAsync(messages, cancellationToken);
            }
        }
    }
}