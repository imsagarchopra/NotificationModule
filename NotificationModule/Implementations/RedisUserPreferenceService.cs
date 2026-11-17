// Redis Preference Engine
public class RedisUserPreferenceService : IUserPreferenceService
{
    // Simulating Redis set: user-2 and user-500 have opted out of promotional emails
    private readonly HashSet<string> _optedOutUsers = new() { "user-2", "user-500" };

    public Task<List<RecipientInfo>> FilterAllowedRecipientsAsync(List<RecipientInfo> recipients, string channelType, NotificationType notificationType)
    {
        // Security Transactional OTPs bypass preference checks
        if (notificationType == NotificationType.Transactional)
        {
            return Task.FromResult(recipients);
        }

        // Filter out opted-out users via Redis set check O(1)
        var allowed = recipients.Where(r => !_optedOutUsers.Contains(r.UserId)).ToList();
        
        int filteredCount = recipients.Count - allowed.Count;
        if (filteredCount > 0)
        {
            Console.WriteLine($"      [REDIS OPT-OUT] Filtered out {filteredCount} user(s) who opted out.");
        }

        return Task.FromResult(allowed);
    }
}