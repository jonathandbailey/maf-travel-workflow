using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Workflows.Dto;
using Workflows.Interfaces;

namespace Travel.Workflows.Repository;

public class CheckpointRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : ICheckpointRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(Guid userId, Guid sessionId , StoreStateDto storeState)
    {
        var serializedConversation = JsonSerializer.Serialize(storeState, SerializerOptions);

        await repository.UploadTextBlobAsync(GetCheckpointFileName(userId, sessionId,
            storeState.CheckpointInfo.CheckpointId, 
            storeState.CheckpointInfo.RunId), 
            settings.Value.ContainerName, 
            serializedConversation, ApplicationJsonContentType);
    }

    public async Task<StoreStateDto> LoadAsync(Guid userId, Guid sessionid, string checkpointId, string runId)
    {
        var blob = await repository.DownloadTextBlobAsync(GetCheckpointFileName(userId, sessionid, checkpointId, runId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {checkpointId}, {runId}");

        
        return stateDto;
    }

    public async Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId)
    {
        var blob = await repository.DownloadTextBlobAsync(GetCheckpointFileName(threadId, checkpointId, runId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {checkpointId}, {runId}");


        return stateDto;
    }

    public async Task<List<StoreStateDto>> GetAsync(Guid userId, Guid sessionId, string runId)
    {
        var blobNames = await repository.ListBlobsAsync(settings.Value.ContainerName, $"{runId}/");
        var checkpoints = new List<StoreStateDto>();

        foreach (var blobName in blobNames)
        {
            var blob = await repository.DownloadTextBlobAsync(blobName, settings.Value.ContainerName);
            var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

            if (stateDto == null)
                throw new JsonException($"Failed to deserialize Checkpoint Store from blob: {blobName}");

            checkpoints.Add(stateDto);
        }

        return checkpoints;
    }

    public async Task<List<StoreStateDto>> GetAsync(string runId)
    {
        var blobNames = await repository.ListBlobsAsync(settings.Value.ContainerName, $"{runId}/");
        var checkpoints = new List<StoreStateDto>();

        foreach (var blobName in blobNames)
        {
            var blob = await repository.DownloadTextBlobAsync(blobName, settings.Value.ContainerName);
            var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

            if (stateDto == null)
                throw new JsonException($"Failed to deserialize Checkpoint Store from blob: {blobName}");

            checkpoints.Add(stateDto);
        }

        return checkpoints;
    }

    public async Task SaveAsync(Guid threadId, StoreStateDto storeState)
    {
        var serializedConversation = JsonSerializer.Serialize(storeState, SerializerOptions);

        await repository.UploadTextBlobAsync(GetCheckpointFileName(threadId,
                storeState.CheckpointInfo.CheckpointId,
                storeState.CheckpointInfo.RunId),
            settings.Value.ContainerName,
            serializedConversation, ApplicationJsonContentType);
    }

    private static string GetCheckpointFileName(Guid userId, Guid sessionId, string checkpointId, string runId)
    {
        return $"{userId}/{sessionId}/checkpoints/{runId}/{checkpointId}.json";
    }

    private static string GetCheckpointFileName(Guid threadId, string checkpointId, string runId)
    {
        return $"{threadId}/checkpoints/{runId}/{checkpointId}.json";
    }
}