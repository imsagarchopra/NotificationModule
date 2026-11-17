public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=========================================================================");
        Console.WriteLine("   COMPLETE ENTERPRISE NOTIFICATION SYSTEM END-TO-END DEMO");
        Console.WriteLine("=========================================================================\n");

        // 1. Initialize Infrastructure & Services
        IMessageBroker kafkaBroker = new InMemoryKafkaBroker();
        IUserRepository userRepository = new SqlUserRepository(); // Has 12,000 users
        IUserPreferenceService preferenceService = new RedisUserPreferenceService();
        INotificationValidator validator = new NotificationValidator();
        ITemplateEngine templateEngine = new TemplateEngine();
        
        var channels = new List<INotificationChannel> { new EmailNotificationChannel() };

        // 2. Build Orchestrator & Ingestion Worker
        INotificationOrchestrator orchestrator = new NotificationOrchestrator(channels, validator, preferenceService, templateEngine);
        IngestionWorker worker = new IngestionWorker(kafkaBroker, userRepository, orchestrator);

        // 3. Start the Ingestion Worker on a background task
        using var cts = new CancellationTokenSource();
        Task workerTask = Task.Run(() => worker.StartProcessingAsync(cts.Token));

        // =========================================================================
        // STEP A: Upstream Auth Service Publishes a Single High-Priority OTP
        // =========================================================================
        Console.WriteLine("\n---> [PUBLISHER: Auth Service] Publishing Single OTP Event...");
        
        await kafkaBroker.PublishAsync(new EnterpriseNotificationEvent(
            EventId: "EVT-OTP-101",
            TemplateId: "otp-template",
            ChannelTypes: ["Email", "SMS"],
            NotificationType: NotificationType.Transactional,
            Priority: NotificationPriority.High,
            TargetAudienceSegment: "SingleUser",
            SingleRecipient: new RecipientInfo("user-sagar", "sagar@omni.com"),
            PayloadTokens: new Dictionary<string, string> { { "OtpCode", "554433" } }
        ));

        await Task.Delay(500); // Give worker time to process

        // =========================================================================
        // STEP B: Upstream Marketing Service Publishes Diwali Sale for 12,000 Users
        // =========================================================================
        Console.WriteLine("\n---> [PUBLISHER: Marketing Service] Publishing Diwali Sale Event for ALL 12,000 Users...");
        
        await kafkaBroker.PublishAsync(new EnterpriseNotificationEvent(
            EventId: "EVT-DIWALI-2026",
            TemplateId: "diwali-sale-template",
            ChannelTypes: ["Email", "SMS"],
            NotificationType: NotificationType.Promotional,
            Priority: NotificationPriority.Low,
            TargetAudienceSegment: "AllActiveUsers",
            SingleRecipient: null, // Null means bulk segment
            PayloadTokens: new Dictionary<string, string>()
        ));

        await Task.Delay(1500); // Give worker time to process 12,000 users in batches

        // Clean shutdown
        cts.Cancel();
        try { await workerTask; } catch (OperationCanceledException) { }
        
        Console.WriteLine("\n=========================================================================");
        Console.WriteLine("   DEMO COMPLETE: All Events Processed Successfully.");
        Console.WriteLine("=========================================================================");
    }
}