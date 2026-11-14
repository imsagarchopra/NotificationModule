using System.ComponentModel.DataAnnotations;

public interface INotificationValidator
{
    ValidationResult Validate(NotificationEvent notificationEvent);
}