namespace Application.Agents.Repository;

public class AgentTemplateRepository : IAgentTemplateRepository
{
    public async Task<string> Load(string key)
    {
        var promptsDirectory = Path.Combine(AppContext.BaseDirectory, "Agents/Templates");

        var templatePath = Path.Combine(promptsDirectory, $"{key}.md");
        
        var template = await File.ReadAllTextAsync(templatePath);
    
        return template;
    }
}

public interface IAgentTemplateRepository
{
    Task<string> Load(string key);
}