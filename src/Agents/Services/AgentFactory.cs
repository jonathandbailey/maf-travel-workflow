using System.ClientModel;
using Agents.Repository;
using Agents.Settings;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;


namespace Agents.Services;

public class AgentFactory : IAgentFactory
{
    private readonly IAgentTemplateRepository _templateRepository;
    private readonly IAgentMiddlewareFactory _agentMiddlewareFactory;
    private readonly ChatClient _chatClient;

    public AgentFactory(IAgentTemplateRepository templateRepository,
        IOptions<LanguageModelSettings> settings, IAgentMiddlewareFactory agentMiddlewareFactory)
    {
        _templateRepository = templateRepository;
        _agentMiddlewareFactory = agentMiddlewareFactory;

        _chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);
    }

    public async Task<AIAgent> Create(
        string name, 
        ChatResponseFormat? chatResponseFormat = null, 
        List<AITool>? tools = null)
    {
        var template = await _templateRepository.Load(name);
    
        ChatOptions chatOptions = new()
        {
            ResponseFormat = chatResponseFormat,
            Instructions = template,
            Tools = tools
        };
      
        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = name,
            
            ChatOptions = chatOptions
        };

        var agent = _chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        return agent;
    }

    public AIAgent UseMiddleware(AIAgent agent, string name)
    {
        var middleware = _agentMiddlewareFactory.Get(name);
        
        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: middleware.RunAsync, runStreamingFunc: middleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }
}

public interface IAgentFactory
{
    Task<AIAgent> Create(string name, ChatResponseFormat? chatResponseFormat = null, List<AITool>? tools = null);
    AIAgent UseMiddleware(AIAgent agent, string name);
}

