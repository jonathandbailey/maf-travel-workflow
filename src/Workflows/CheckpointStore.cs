using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Workflows.Dto;
using Workflows.Interfaces;

namespace Workflows;

public class CheckpointStore(ICheckpointRepository checkpointRepository, Guid threadId) : JsonCheckpointStore
{
    public override async ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        var stateByRunId = await checkpointRepository.GetAsync(runId);

        return stateByRunId.Select(x => x.CheckpointInfo);
    }

    public override async ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        await checkpointRepository.SaveAsync(threadId, new StoreStateDto(checkpointInfo, value));
       
        return checkpointInfo;
    }

    public override async ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var stateDto = await checkpointRepository.LoadAsync(threadId, key.CheckpointId, runId);

        return stateDto.JsonElement;
    }
}