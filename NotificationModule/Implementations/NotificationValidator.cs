public class NotificationValidator : INotificationValidator
{
    public ValidationResult Validate(NotificationEvent notificationEvent)
    {
        //any validation logic
        if(notificationEvent.Recipients == null || !notificationEvent.Recipients.Any())
        {
            return ValidationResult.Failure("Recipients cannot be null or empty.");
        }

        // Rule: Transactional messages MUST be High priority
        if(notificationEvent.NotificationType == NotificationType.Transactional && notificationEvent.Priority != NotificationPriority.High)
        {
            return ValidationResult.Failure("Transactional notifications must have high priority.");
        }
        
        return ValidationResult.Success();

    }
}
