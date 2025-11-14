using Application.Observability;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Workflows.ReAct;
using Application.Workflows.ReAct.Dto;

namespace Application.Infrastructure;

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
  
    public async Task SaveAsync(Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo)
    {
        using var activity = Telemetry.Start("WorkflowManager-[save]");

        var workflowStateDto = new WorkflowStateDto(state, checkpointInfo);
        
        var serializedConversation = JsonSerializer.Serialize(workflowStateDto, SerializerOptions);

        await repository.UploadTextBlobAsync($"{sessionId}.json", settings.Value.ContainerName,
            serializedConversation, ApplicationJsonContentType);
    }

    public async Task<WorkflowStateDto> LoadAsync(Guid sessionId)
    {
        using var activity = Telemetry.Start("WorkflowManager-[initialize]");

        var blobExists = await repository.BlobExists($"{sessionId}.json", settings.Value.ContainerName);

        if (blobExists == false)
        {
            return new WorkflowStateDto(WorkflowState.Initialized, null);
        }

        var blob = await repository.DownloadTextBlobAsync($"{sessionId}.json", settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<WorkflowStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {sessionId}");

        return stateDto;
    }
}

public interface IWorkflowRepository
{
    Task SaveAsync(Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo);
   
    Task<WorkflowStateDto> LoadAsync(Guid sessionId);
}