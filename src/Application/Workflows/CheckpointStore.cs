using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Text.Json;
using Application.Infrastructure;
using Application.Workflows.Conversations.Dto;

namespace Application.Workflows;

public class CheckpointStore(ICheckpointRepository checkpointRepository) : JsonCheckpointStore
{
    private readonly Dictionary<CheckpointInfo, JsonElement> _checkpointElements = new();
    
    public override ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        return ValueTask.FromResult<IEnumerable<CheckpointInfo>>(_checkpointElements.Keys.ToList());
    }

    public override async ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        await checkpointRepository.SaveAsync(new StoreStateDto(checkpointInfo, value));

        _checkpointElements.Add(checkpointInfo, value);
       
        return checkpointInfo;
    }

    public override async ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var stateDto = await checkpointRepository.LoadAsync(key.CheckpointId, runId);

        return stateDto.JsonElement;
    }
}