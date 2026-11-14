// STEP 6: User Preference Engine (Redis Simulation)
public class RedisUserPreferenceService : IUserPreferenceService
{
    // Simulated Redis cache of opted-out users
    private readonly HashSet<string> _optedOutUsers = new() { "user-2" }; // user-2 opted out of promo emails

    public async Task<List<RecipientInfo>> FilterAllowedRecipientsAsync(List<RecipientInfo> recipients, string channelType, NotificationType notificationType)
    {
        await Task.Yield(); // Simulate fast async O(1) Redis lookup

        // Transactional OTPs bypass user preferences (Mandatory Security Alerts)
        if (notificationType == NotificationType.Transactional)
        {
            return recipients;
        }

        // Filter out users who opted out of Promotional messages
        var allowedRecipients = recipients.Where(r => !_optedOutUsers.Contains(r.UserId)).ToList();
        
        int droppedCount = recipients.Count - allowedRecipients.Count;
        if (droppedCount > 0)
        {
            Console.WriteLine($"   [PREFERENCE CHECK] Filtered out {droppedCount} opted-out user(s) via Redis cache.");
        }

        return allowedRecipients;
    }
}
