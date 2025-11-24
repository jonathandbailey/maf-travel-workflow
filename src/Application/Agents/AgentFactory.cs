using Application.Agents.Repository;
using Application.Settings;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using Azure.Identity;

namespace Application.Agents;

public class AgentFactory(IAgentTemplateRepository templateRepository, IOptions<LanguageModelSettings> settings) : IAgentFactory
{
    public async Task<IAgent> CreateReasonAgent()
    {
        return await Create("Reason-Agent");
    }

    public async Task<IAgent> CreateActAgent()
    {
        return await Create("Act-Agent");
    }

    public async Task<IAgent> CreateOrchestrationAgent()
    {
        return await Create("Orchestration-Agent");
    }

    public async Task<IAgent> CreateFlightWorkerAgent()
    {
        return await Create("Flight-Agent");
    }

    public async Task<IAgent> CreateHotelWorkerAgent()
    {
        return await Create("Hotel-Agent");
    }

    public async Task<IAgent> CreateTrainWorkerAgent()
    {
        return await Create("Train-Agent");
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
    Task<IAgent> CreateReasonAgent();
    Task<IAgent> CreateActAgent();
    Task<IAgent> CreateOrchestrationAgent();
    Task<IAgent> CreateFlightWorkerAgent();
    Task<IAgent> CreateHotelWorkerAgent();
    Task<IAgent> CreateTrainWorkerAgent();
}