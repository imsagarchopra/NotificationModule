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

    public async Task DispatchAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"\n[ORCHESTRATOR] Received event '{notificationEvent.EventId}' for Channel: {notificationEvent.ChannelType}");

        // 1. VALIDATION CHECK
        var validationResult = _validator.Validate(notificationEvent);
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"   ❌ [REJECTED BY VALIDATION] {validationResult.ErrorMessage}");
            return;
        }
        Console.WriteLine("   ✅ [VALIDATION PASSED]");

        // 2. USER PREFERENCE CHECK (Filtering Opt-outs)
        var validRecipients = await _preferenceService.FilterAllowedRecipientsAsync(
            notificationEvent.Recipients, 
            notificationEvent.ChannelType, 
            notificationEvent.NotificationType
        );

        if (!validRecipients.Any())
        {
            Console.WriteLine("   ⚠️ [ABORTED] All recipients in this batch have opted out.");
            return;
        }

        // 3. CHANNEL RESOLUTION
        if (!_channels.TryGetValue(notificationEvent.ChannelType, out var channel))
        {
            throw new NotSupportedException($"Channel '{notificationEvent.ChannelType}' is not registered.");
        }

        // 4. TEMPLATE RENDERING
        (string subject, string body) = await _templateEngine.RenderAsync(notificationEvent.TemplateId, notificationEvent.PayloadTokens);

        // 5. DISPATCH EXECUTION (Single vs. Batch)
        if (notificationEvent.Priority == NotificationPriority.High)
        {
            // Transactional / OTP Path
            foreach (var recipient in validRecipients)
            {
                var message = new NotificationMessage(recipient.Address, subject, body);
                await channel.SendAsync(message, cancellationToken);
            }
        }
        else
        {
            // Promotional Bulk Batch Path
            var messages = validRecipients.Select(r => new NotificationMessage(r.Address, subject, body));
            await channel.SendBatchAsync(messages, cancellationToken);
        }
    }
}