using Application.Workflows.ReAct.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Infrastructure;

public class AgentThreadRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IAgentThreadRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(Guid userId, Guid sessionId, AgentState state, string name)
    {
        var serializedConversation = JsonSerializer.Serialize(state, SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetCheckpointFileName(userId, sessionId, name), 
            settings.Value.ContainerName,
            serializedConversation, 
            ApplicationJsonContentType);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid sessionId, string name)
    {
        return await repository.BlobExists(GetCheckpointFileName(userId, sessionId, name), settings.Value.ContainerName);
    }

    public async Task<AgentState> LoadAsync(Guid userId, Guid sessionId, string name)
    {
        var blob = await repository.DownloadTextBlobAsync(GetCheckpointFileName(userId, sessionId, name), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<AgentState>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {name}");


        return stateDto;
    }

    private static string GetCheckpointFileName(Guid userId, Guid sessionId, string name)
    {
        return $"{userId}/{sessionId}/agents/{name}.json";
    }
}

public interface IAgentThreadRepository
{
    Task SaveAsync(Guid userId, Guid sessionId, AgentState state, string name);
    Task<bool> ExistsAsync(Guid userId, Guid sessionId, string name);
    Task<AgentState> LoadAsync(Guid userId, Guid sessionId, string name);
}

public class AgentState(JsonElement thread)
{
    public JsonElement Thread { get; } = thread;
}