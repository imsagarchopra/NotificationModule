public class TemplateEngine : ITemplateEngine
{
    public async Task<(string Subject, string Body)> RenderAsync(string templateId, Dictionary<string, string> tokens)
    {
        await Task.Yield();
        
        if (templateId == "otp-template")
        {
            string code = tokens.GetValueOrDefault("OtpCode", "000000");
            return ("Your Security OTP", $"Your one-time passcode is {code}.");
        }

        return ("Diwali Festival Offer!", "Enjoy 50% OFF on all enterprise software subscriptions!");
    }
}
