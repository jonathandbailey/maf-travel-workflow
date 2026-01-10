using System.ClientModel;
using Application.Agents.Middleware;
using Application.Agents.Repository;
using Application.Settings;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Application.Services;
using Application.Workflows.Dto;

namespace Application.Agents;

public class AgentFactory(
    IAgentTemplateRepository templateRepository, 
    IAgentMemoryMiddleware agentMemoryMiddleware,
    IOptions<LanguageModelSettings> settings) : IAgentFactory
{
 
    public async Task<AIAgent> CreateFlightAgent()
    {
        var template = await templateRepository.Load("Flight-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);
     
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(FlightActionResultDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "FlightPlan",
                schemaDescription: "User Flight Options for their vacation.")
        };

        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "flight_agent",
            Instructions = template,
            ChatOptions = chatOptions
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: agentMemoryMiddleware.RunAsync, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    public async Task<AIAgent> CreateReasonAgent()
    {
        var template = await templateRepository.Load("Reason-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var schema = AIJsonUtilities.CreateJsonSchema(typeof(ReasoningOutputDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "ReasoningActRequest",
                schemaDescription: "Reasoning State for Act.")
        };
      
        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "reason_agent",
            Instructions = template,
            ChatOptions = chatOptions
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: agentMemoryMiddleware.RunAsync, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    public async Task<AIAgent> CreateConversationAgent(ITravelWorkflowService travelWorkflowService)
    {
        var template = await templateRepository.Load("Conversation-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "conversation_agent",
            Instructions = template,
            ChatOptions = new ChatOptions {Tools = [AIFunctionFactory.Create(travelWorkflowService.PlanVacation)] }
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options:clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: null, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    private static ChatOptions CreateReasonChatOptions()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(ReasoningOutputDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "ReasoningActRequest",
                schemaDescription: "Reasoning State for Act.")
        };

        return chatOptions;
    }

    private static ChatOptions CreateFlightChatOptions()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(FlightActionResultDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "FlightPlan",
                schemaDescription: "User Flight Options for their vacation.")
        };

        return chatOptions;
    }

    private static ChatOptions CreateParserChatOptions()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(UserParsedDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "HotelPlan",
                schemaDescription: "User Hotel Options for their vacation.")
        };

        return chatOptions;
    }

    private static ChatOptions CreateHotelChatOptions()
    {
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(HotelSearchResultDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "ParsedTravelPlan",
                schemaDescription: "The parsed travel plan from the user.")
        };

        return chatOptions;
    }
}

public interface IAgentFactory
{
    Task<AIAgent> CreateConversationAgent(ITravelWorkflowService travelWorkflowService);
    Task<AIAgent> CreateReasonAgent();
    Task<AIAgent> CreateFlightAgent();
}

public enum AgentTypes
{
    Reason,
    Act,
    Orchestration,
    FlightWorker,
    HotelWorker,
    User,
    Parser,
    Conversation
}

public enum AgentMemoryTypes
{
    Reason,
    Act,
    Orchestration,
    FlightWorker,
    HotelWorker,
    User,
    Parser,
    UserShared,
    Conversation
}