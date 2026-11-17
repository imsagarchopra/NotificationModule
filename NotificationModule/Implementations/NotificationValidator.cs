// Validation Pipeline
public class NotificationValidator : INotificationValidator
{
    public ValidationResult Validate(EnterpriseNotificationEvent notificationEvent)
    {
        if (string.IsNullOrWhiteSpace(notificationEvent.TemplateId))
            return ValidationResult.Failure("TemplateId is required.");

        if (notificationEvent.NotificationType == NotificationType.Transactional && notificationEvent.Priority != NotificationPriority.High)
            return ValidationResult.Failure("Transactional notifications must be High Priority.");

        return ValidationResult.Success();
    }
}