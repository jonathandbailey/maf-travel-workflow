using Application.Agents.Repository;
using Application.Infrastructure;
using Application.Settings;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.Text.Json;
using Application.Workflows.ReWoo.Dto;

namespace Application.Agents;

public class AgentFactory(IAgentTemplateRepository templateRepository, IAgentThreadRepository agentThreadRepository, IOptions<LanguageModelSettings> settings) : IAgentFactory
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

    private readonly Dictionary<AgentTypes, ChatOptions> _agentChatOptions = new()
    {
        { AgentTypes.FlightWorker, CreateFlightChatOptions() },
        { AgentTypes.Reason, new ChatOptions() },
        { AgentTypes.Act, new ChatOptions() },
        { AgentTypes.Orchestration, new ChatOptions() },
        { AgentTypes.HotelWorker, new ChatOptions() },
        { AgentTypes.TrainWorker, new ChatOptions() }
    };
    public async Task<IAgent> Create(AgentTypes agentType)
    {
        if (_agentTemplates.TryGetValue(agentType, out var templateName))
        {
            return await Create(templateName, agentType);
        }
        throw new ArgumentException($"Agent type {agentType} is not recognized.");
    }

    private async Task<IAgent> Create(string templateName, AgentTypes type)
    {
        var template = await templateRepository.Load(templateName);

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new DefaultAzureCredential())
            .GetChatClient(settings.Value.DeploymentName);

        var reasonAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = template,
            ChatOptions = _agentChatOptions[type]
        });

        return new Agent(reasonAgent, agentThreadRepository, type);
    }

    private static ChatOptions CreateFlightChatOptions()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(FlightSearchResultDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "FlightPlan",
                schemaDescription: "User Flight Options for their vacation.")
        };

        return chatOptions;
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