using System.ClientModel;
using Agents.Dto;
using Agents.Middleware;
using Agents.Repository;
using Agents.Settings;
using Azure.AI.OpenAI;
using Infrastructure.Dto;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;


namespace Agents;

public class AgentFactory : IAgentFactory
{
    private readonly IAgentTemplateRepository _templateRepository;
    private readonly IAgentMemoryMiddleware _agentMemoryMiddleware;
    private readonly IAgentAgUiMiddleware _agentAgUiMiddleware;
    private readonly ChatClient _chatClient;

    public AgentFactory(IAgentTemplateRepository templateRepository, 
        IAgentMemoryMiddleware agentMemoryMiddleware,
        IAgentAgUiMiddleware agentAgUiMiddleware,
        IOptions<LanguageModelSettings> settings)
    {
        _templateRepository = templateRepository;
        _agentMemoryMiddleware = agentMemoryMiddleware;
        _agentAgUiMiddleware = agentAgUiMiddleware;

        _chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);
    }

    public async Task<AIAgent> Create(string name, ChatResponseFormat? chatResponseFormat = null, List<AITool>? tools = null)
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

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: _agentMemoryMiddleware.RunAsync, runStreamingFunc: _agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    public async Task<AIAgent> Create(string name, List<AITool> tools)
    {
        var template = await _templateRepository.Load(name);

        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = name,

            ChatOptions = new ChatOptions
            {
                Tools = tools,
                Instructions = template
            }
        };

        var agent = _chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        return agent;
    }

    public AIAgent ExtendConversationAgent(AIAgent agent)
    {
        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: null, runStreamingFunc: _agentAgUiMiddleware.RunStreamingAsync)
            .Use(runFunc: null, runStreamingFunc: _agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }
}

public interface IAgentFactory
{
    Task<AIAgent> Create(string name, ChatResponseFormat? chatResponseFormat = null, List<AITool>? tools = null);
    Task<AIAgent> Create(string name, List<AITool> tools);
    AIAgent ExtendConversationAgent(AIAgent agent);
}

