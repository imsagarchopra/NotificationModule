// The Master Orchestrator (Executes Validation, Preferences, Templates, & Channels)
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

    public async Task DispatchBatchAsync(EnterpriseNotificationEvent notificationEvent, List<RecipientInfo> batchRecipients, CancellationToken cancellationToken = default)
    {
        // 1. VALIDATION CHECK
        var validationResult = _validator.Validate(notificationEvent);
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"   ❌ [REJECTED BY VALIDATION] {validationResult.ErrorMessage}");
            return;
        }

        // 2. USER PREFERENCE CHECK (Filter Opt-outs via Redis)
        var allowedRecipients = await _preferenceService.FilterAllowedRecipientsAsync(
            batchRecipients, 
            notificationEvent.ChannelType, 
            notificationEvent.NotificationType
        );

        if (!allowedRecipients.Any())
        {
            Console.WriteLine("   ⚠️ [ABORTED] All recipients in this batch opted out.");
            return;
        }

        // 3. CHANNEL RESOLUTION
        if (!_channels.TryGetValue(notificationEvent.ChannelType, out var channel))
        {
            throw new NotSupportedException($"Channel '{notificationEvent.ChannelType}' is not registered.");
        }

        // 4. TEMPLATE RENDERING
        var (subject, body) = await _templateEngine.RenderAsync(notificationEvent.TemplateId, notificationEvent.PayloadTokens);

        // 5. DISPATCH EXECUTION
        if (notificationEvent.Priority == NotificationPriority.High)
        {
            // Transactional OTP: Send individually
            foreach (var recipient in allowedRecipients)
            {
                var message = new NotificationMessage(recipient.Address, subject, body);
                await channel.SendAsync(message, cancellationToken);
            }
        }
        else
        {
            // Promotional Bulk: Send 1 Batch API call for the 5,000 users
            var messages = allowedRecipients.Select(r => new NotificationMessage(r.Address, subject, body));
            await channel.SendBatchAsync(messages, cancellationToken);
        }
    }
}