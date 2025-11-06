using Application.Agents;
using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Conversations;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Infrastructure.Settings;
using Application.Workflows.Conversations.Dto;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory, IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IApplicationService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var initializeActivity = Telemetry.StarActivity("Initialize");

        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();

        var store = new ConversationCheckpointStore();

        var checkpointManager = CheckpointManager.CreateJson(store);

        initializeActivity?.Dispose();

        var workflowActivity = Telemetry.StarActivity("Workflow");

        workflowActivity?.SetTag("User Input", request.Message);

        var workflow = new ConversationWorkflow(reasonAgent, actAgent, checkpointManager);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, request.Message));

        workflowActivity?.Dispose();

        var serializedConversation = JsonSerializer.Serialize(store, SerializerOptions);

        await repository.UploadTextBlobAsync($"{request.SessionId}.json", settings.Value.ContainerName,
            serializedConversation, ApplicationJsonContentType);

        return new ConversationResponse(request.SessionId, response.Message);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}