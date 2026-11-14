public interface IUserPreferenceService
{
    // Checks Redis cache to filter out users who opted out of this channel/type
    Task<List<RecipientInfo>> FilterAllowedRecipientsAsync(List<RecipientInfo> recipients, string channelType, NotificationType notificationType);
}
