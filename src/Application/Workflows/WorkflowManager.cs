using Api.Infrastructure.Settings;
using Application.Infrastructure;
using Application.Workflows.Conversations;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows;

public class WorkflowManager(ICheckpointStore<JsonElement> checkpointStore, IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IWorkflowManager
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ICheckpointStore<JsonElement> _checkpointStore = checkpointStore;
    public CheckpointManager CheckpointManager { get; private set; } 

    public async Task Initialize(Guid sessionId)
    {
        var store = await GetOrCreateCheckpointStore(sessionId);

        CheckpointManager = CheckpointManager.CreateJson(store);
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

public interface IWorkflowManager
{
    CheckpointManager CheckpointManager { get; }
}