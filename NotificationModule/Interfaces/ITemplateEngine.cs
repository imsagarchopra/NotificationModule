public interface ITemplateEngine
{
    Task<(string Subject, string Body)> RenderAsync(string templateId, Dictionary<string,string> tokens);
}
