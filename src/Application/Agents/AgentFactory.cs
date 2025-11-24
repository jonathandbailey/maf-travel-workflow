using Application.Agents.Repository;
using Application.Settings;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using Azure.Identity;

namespace Application.Agents;

public class AgentFactory(IAgentTemplateRepository templateRepository, IOptions<LanguageModelSettings> settings) : IAgentFactory
{
    private readonly Dictionary<AgentTypes, string> _agentTemplates = new()
    {
        { AgentTypes.Reason, "Reason-Agent" },
        { AgentTypes.Act, "Act-Agent" },
        { AgentTypes.Orchestration, "Orchestration-Agent" },
        { AgentTypes.FlightWorker, "Flight-Agent" },
        { AgentTypes.HotelWorker, "Hotel-Agent" },
        { AgentTypes.TrainWorker, "Train-Agent" }
    };

    public async Task<IAgent> Create(AgentTypes agentType)
    {
        if (_agentTemplates.TryGetValue(agentType, out var templateName))
        {
            return await Create(templateName);
        }
        throw new ArgumentException($"Agent type {agentType} is not recognized.");
    }

    private async Task<IAgent> Create(string templateName)
    {
        var template = await templateRepository.Load(templateName);
    
        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new DefaultAzureCredential())
            .GetChatClient(settings.Value.DeploymentName);

        var reasonAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = template
        });

        return new Agent(reasonAgent);
    }
}

public interface IAgentFactory
{
    Task<IAgent> Create(AgentTypes agentType);
}

public enum AgentTypes
{
    Reason,
    Act,
    Orchestration,
    FlightWorker,
    HotelWorker,
    TrainWorker
}