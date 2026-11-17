// THIS IS THE INGESTION WORKER YOU ASKED FOR
// In production, this inherits from BackgroundService in .NET
public class IngestionWorker
{
    private readonly IMessageBroker _messageBroker;
    private readonly IUserRepository _userRepository;
    private readonly INotificationOrchestrator _orchestrator;
    private const int BatchSize = 5000; // Process in chunks of 5,000 to prevent OOM

    public IngestionWorker(
        IMessageBroker messageBroker, 
        IUserRepository userRepository, 
        INotificationOrchestrator orchestrator)
    {
        _messageBroker = messageBroker;
        _userRepository = userRepository;
        _orchestrator = orchestrator;
    }

    // The Worker Loop (Continuously running background thread)
    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[INGESTION WORKER] Background Worker Started. Listening for queue events...");

        while (!cancellationToken.IsCancellationRequested)
        {
            // 1. Poll event from Kafka
            var notificationEvent = await _messageBroker.ConsumeAsync(cancellationToken);
            
            if (notificationEvent == null)
            {
                await Task.Delay(100, cancellationToken); // Sleep briefly if queue is empty
                continue;
            }

            Console.WriteLine($"\n[INGESTION WORKER] Picked up Event '{notificationEvent.EventId}' from Kafka.");

            // 2. Handle Scenario A: High Priority Single User (OTP)
            if (notificationEvent.SingleRecipient != null)
            {
                var singleBatch = new List<RecipientInfo> { notificationEvent.SingleRecipient };
                await _orchestrator.DispatchBatchAsync(notificationEvent, singleBatch, cancellationToken);
                continue;
            }

            // 3. Handle Scenario B: Bulk Sale (Millions of Users)
            // Query DB in paginated chunks of 5,000 to avoid loading millions into RAM
            int totalUsers = await _userRepository.GetTotalUsersInSegmentAsync(notificationEvent.TargetAudienceSegment);
            int totalPages = (int)Math.Ceiling((double)totalUsers / BatchSize);

            Console.WriteLine($"[INGESTION WORKER] Target Segment has {totalUsers} users. Splitting into {totalPages} batch chunk(s) of {BatchSize}.");

            for (int page = 0; page < totalPages; page++)
            {
                // Fetch chunk of 5,000 users from DB
                List<RecipientInfo> userChunk = await _userRepository.GetUsersBySegmentInBatchesAsync(
                    notificationEvent.TargetAudienceSegment, 
                    page, 
                    BatchSize
                );

                Console.WriteLine($"\n   [INGESTION WORKER] Processing Batch #{page + 1} with {userChunk.Count} users...");
                
                // Pass this chunk of 5,000 directly to the Orchestrator
                await _orchestrator.DispatchBatchAsync(notificationEvent, userChunk, cancellationToken);
            }
        }
    }
}