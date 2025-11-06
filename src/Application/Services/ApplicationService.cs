using Application.Agents;
using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Conversations;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Infrastructure.Settings;
using Application.Workflows;
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

        var store = await GetOrCreateCheckpointStore(request.SessionId);
      
        initializeActivity?.Dispose();

        var workflowActivity = Telemetry.StarActivity("Workflow");

        workflowActivity?.SetTag("User Input", request.Message);

        var workflowManager = new WorkflowManager(store, repository, settings);

        await workflowManager.Initialize(request.SessionId);

        var workflow = new ConversationWorkflow(reasonAgent, actAgent, workflowManager);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, request.Message));

        workflowActivity?.Dispose();

        var serializedConversation = JsonSerializer.Serialize(store, SerializerOptions);

        await repository.UploadTextBlobAsync($"{request.SessionId}.json", settings.Value.ContainerName,
            serializedConversation, ApplicationJsonContentType);

        return new ConversationResponse(request.SessionId, response.Message);
    }

    private async Task<ConversationCheckpointStore> GetOrCreateCheckpointStore(Guid sessionId)
    {
        var blobExists = await repository.BlobExists($"{sessionId}.json", settings.Value.ContainerName);

        if (blobExists == false)
        {
            return new ConversationCheckpointStore();
        }

        var blob = await repository.DownloadTextBlobAsync($"{sessionId}.json", settings.Value.ContainerName);

        var store = JsonSerializer.Deserialize<ConversationCheckpointStore>(blob, SerializerOptions);

        if (store == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {sessionId}");

        return store;
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}