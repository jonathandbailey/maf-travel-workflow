using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Xunit.Abstractions;

namespace Tests;

public class FakeCheckpointStore(ITestOutputHelper outputHelper) : JsonCheckpointStore
{
    private readonly Dictionary<CheckpointInfo, JsonElement> _checkpointElements = new();

    public override ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        return ValueTask.FromResult<IEnumerable<CheckpointInfo>>(_checkpointElements.Keys.ToList());
    }

    public override ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        _checkpointElements.Add(checkpointInfo, value);
        
        outputHelper.WriteLine($"Created: {DateTime.Now} {checkpointInfo}");

        return ValueTask.FromResult(checkpointInfo);
    }

    public override ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var element = _checkpointElements[key];

        return ValueTask.FromResult(element);
    }
}