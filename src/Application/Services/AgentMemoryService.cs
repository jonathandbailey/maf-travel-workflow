using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Application.Users;
using Infrastructure.Settings;

namespace Application.Services;

public class AgentMemoryService(IAzureStorageRepository repository, IExecutionContextAccessor sessionContextAccessor, IOptions<AzureStorageSeedSettings> settings) : IAgentMemoryService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(AgentState state, string name)
    {
        var serializedConversation = JsonSerializer.Serialize(state, SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetStorageFileName(name), 
            settings.Value.ContainerName,
            serializedConversation, 
            ApplicationJsonContentType);
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await repository.BlobExists(GetStorageFileName(name), settings.Value.ContainerName);
    }

    public async Task<AgentState> LoadAsync(string name)
    {
        var blob = await repository.DownloadTextBlobAsync(GetStorageFileName(name), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<AgentState>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {name}");


        return stateDto;
    }

    private string GetStorageFileName(string name)
    {
        var userId = sessionContextAccessor.Context.UserId;
        var sessionId = sessionContextAccessor.Context.SessionId;

        return $"{userId}/{sessionId}/agents/{name}.json";
    }
}

public interface IAgentMemoryService
{
    Task SaveAsync(AgentState state, string name);
    Task<bool> ExistsAsync(string name);
    Task<AgentState> LoadAsync(string name);
}

public class AgentState(JsonElement thread)
{
    public JsonElement Thread { get; } = thread;
}