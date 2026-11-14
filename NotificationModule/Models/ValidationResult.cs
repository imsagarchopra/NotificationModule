public record ValidationResult(
    bool IsValid,
    string ErrorMessage
)
{
    public static ValidationResult Success() => new ValidationResult(true, string.Empty);
    public static ValidationResult Failure(string errorMessage) => new ValidationResult(false, errorMessage);
}
