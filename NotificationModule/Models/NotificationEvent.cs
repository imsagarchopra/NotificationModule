public record NotificationEvent(
    string EventId,
    string TemplateId,
    string ChannelType, // "Email", "SMS"
    NotificationType NotificationType,
    NotificationPriority Priority,
    List<RecipientInfo> Recipients,
    Dictionary<string, string> PayloadTokens
);
