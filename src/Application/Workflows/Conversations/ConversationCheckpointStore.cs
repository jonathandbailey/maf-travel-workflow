using Application.Observability;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows.Conversations;

public class ConversationCheckpointStore : JsonCheckpointStore
{
    [JsonConverter(typeof(CheckpointDictionaryConverter))]
    private readonly Dictionary<CheckpointInfo, JsonElement> _checkpointElements = new();

    [JsonConstructor]
    public ConversationCheckpointStore(Dictionary<CheckpointInfo, JsonElement>? checkpointElements = null)
    {
        if (checkpointElements is not null)
        {
            _checkpointElements = checkpointElements;
        }
    }

    [JsonInclude]
    [JsonConverter(typeof(CheckpointDictionaryConverter))]
    public Dictionary<CheckpointInfo, JsonElement> CheckpointElements
    {
        get => _checkpointElements;
        init => _checkpointElements = value;
    }
    
    public override ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        return ValueTask.FromResult<IEnumerable<CheckpointInfo>>(_checkpointElements.Keys.ToList());
    }

    public override ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        using var activity = Telemetry.StarActivity("CheckpointStore-Create");

        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        activity?.SetTag("RunId", checkpointInfo.RunId);
        activity?.SetTag("CheckpointId", checkpointInfo.CheckpointId);

        _checkpointElements.Add(checkpointInfo, value);

        return ValueTask.FromResult(checkpointInfo);
    }

    public override ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var element = _checkpointElements[key];

        return ValueTask.FromResult(element);
    }
}