using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Options;
using Workflows;
using Workflows.Dto;
using Workflows.Interfaces;

namespace Travel.Workflows.Repository;

public class WorkflowRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IWorkflowRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
  
    public async Task SaveAsync(Guid userId, Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo)
    {
        var workflowStateDto = new WorkflowStateDto(state, checkpointInfo);
        
        var serializedWorkflowState = JsonSerializer.Serialize(workflowStateDto, SerializerOptions);

        await repository.UploadTextBlobAsync(GetFileName(userId, sessionId), settings.Value.ContainerName,
            serializedWorkflowState, ApplicationJsonContentType);
    }

    public async Task<WorkflowStateDto> LoadAsync(Guid userId, Guid sessionId)
    {
        var blobExists = await repository.BlobExists(GetFileName(userId, sessionId), settings.Value.ContainerName);

        if (blobExists == false)
        {
            return new WorkflowStateDto(WorkflowState.Initialized, null);
        }

        var blob = await repository.DownloadTextBlobAsync(GetFileName(userId, sessionId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<WorkflowStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {sessionId}");

        return stateDto;
    }

    public async Task SaveAsync(Guid threadId, WorkflowState state, CheckpointInfo? checkpointInfo)
    {
        var workflowStateDto = new WorkflowStateDto(state, checkpointInfo);

        var serializedWorkflowState = JsonSerializer.Serialize(workflowStateDto, SerializerOptions);

        await repository.UploadTextBlobAsync(GetFileName(threadId), settings.Value.ContainerName,
            serializedWorkflowState, ApplicationJsonContentType);
    }

    public async Task<WorkflowStateDto> LoadAsync(Guid threadId)
    {
        var blobExists = await repository.BlobExists(GetFileName(threadId), settings.Value.ContainerName);

        if (blobExists == false)
        {
            return new WorkflowStateDto(WorkflowState.Initialized, null);
        }

        var blob = await repository.DownloadTextBlobAsync(GetFileName(threadId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<WorkflowStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for threadId : {threadId}");

        return stateDto;
    }

    private static string GetFileName(Guid threadId)
    {
        return $"{threadId}/workflows/state.json";
    }

    private static string GetFileName(Guid userId, Guid sessionId)
    {
        return $"{userId}/{sessionId}/workflows/state.json";
    }
}