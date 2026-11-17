
// Upstream event published by Marketing / Order microservices
public record EnterpriseNotificationEvent(
    string EventId,
    string TemplateId,
    //string ChannelType, // "Email", "SMS"
    List<string> ChannelTypes, // e.g., ["Email", "SMS", "Push"]
    NotificationType NotificationType,
    NotificationPriority Priority,
    string TargetAudienceSegment, // e.g., "AllActiveUsers" or "SingleUser"
    RecipientInfo? SingleRecipient, // Populated for 1-to-1 OTPs
    //List<RecipientInfo> Recipients,  Not needed for EnterpriseNotificationEvent, as the recipients will be resolved based on the TargetAudienceSegment
    Dictionary<string, string> PayloadTokens
);