public class Program
{
    public static async Task Main()
    {
        // Setup Dependency Injection & Services
        var channels = new List<INotificationChannel> { new EmailNotificationChannel() };
        var validator = new NotificationValidator();
        var preferences = new RedisUserPreferenceService();
        var templateEngine = new TemplateEngine();

        INotificationOrchestrator orchestrator = new NotificationOrchestrator(channels,validator,preferences,templateEngine);

        // =========================================================================
        // SCENARIO A: Single High-Priority Transactional OTP
        // =========================================================================
        Console.WriteLine("=================================================");
        Console.WriteLine("SCENARIO A: User requests OTP (High Priority)");
        Console.WriteLine("=================================================");

        var otpEvent = new NotificationEvent(
            EventId: "EVT-OTP-99",
            TemplateId: "otp-template",
            ChannelType: "Email",
            NotificationType: NotificationType.Transactional,
            Priority: NotificationPriority.High,
            Recipients: new List<RecipientInfo> { new("user-sagar", "sagar@omni.com") },
            PayloadTokens: new Dictionary<string, string> { { "OtpCode", "987654" } }
        );

        await orchestrator.DispatchAsync(otpEvent);

        // =========================================================================
        // SCENARIO B: Diwali Sale Batch (Low Priority - Millions of Users)
        // =========================================================================
        Console.WriteLine("\n=================================================");
        Console.WriteLine("SCENARIO B: Diwali Sale Batch (Low Priority)");
        Console.WriteLine("=================================================");

        // Ingestion Worker pulls a chunk of 3 users (user-2 has opted out in Redis!)
        var batchRecipients = new List<RecipientInfo>
        {
            new("user-1", "user1@omni.com"),
            new("user-2", "user2@omni.com"), // Will be filtered out by Redis
            new("user-3", "user3@omni.com")
        };

        var diwaliBatchEvent = new NotificationEvent(
            EventId: "EVT-DIWALI-BATCH-001",
            TemplateId: "diwali-promo-template",
            ChannelType: "Email",
            NotificationType: NotificationType.Promotional,
            Priority: NotificationPriority.Low,
            Recipients: batchRecipients,
            PayloadTokens: new Dictionary<string, string>()
        );

        await orchestrator.DispatchAsync(diwaliBatchEvent);
    }
}
