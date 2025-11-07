using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Workflows.Conversations.Dto;

namespace Application.Workflows.Conversations;

public class ConversationCheckpointStore : JsonCheckpointStore
{
    [JsonConverter(typeof(CheckpointDictionaryConverter))]
    private readonly Dictionary<CheckpointInfo, JsonElement> _checkpointElements = new();
    private readonly object _lock = new();

    [JsonConstructor]
    public ConversationCheckpointStore(Dictionary<CheckpointInfo, JsonElement>? checkpointElements = null)
    {
        if (checkpointElements is not null)
        {
            _checkpointElements = checkpointElements;
        }
    }

    public ConversationCheckpointStore(List<StoreStateDto> stateDtos)
    {
        foreach (var storeStateDto in stateDtos)
        {
            _checkpointElements.Add(storeStateDto.CheckpointInfo, storeStateDto.JsonElement);
        }
    }

    [JsonInclude]
    [JsonConverter(typeof(CheckpointDictionaryConverter))]
    public Dictionary<CheckpointInfo, JsonElement> CheckpointElements
    {
        get
        {
            lock (_lock)
            {
                return new Dictionary<CheckpointInfo, JsonElement>(_checkpointElements);
            }
        }
        init => _checkpointElements = value;
    }
    
    public override ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        lock (_lock)
        {
            return ValueTask.FromResult<IEnumerable<CheckpointInfo>>(_checkpointElements.Keys.ToList());
        }
    }

    public override ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        lock (_lock)
        {
            _checkpointElements.Add(checkpointInfo, value);
        }

        return ValueTask.FromResult(checkpointInfo);
    }

    public override ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        lock (_lock)
        {
            var element = _checkpointElements[key];
            return ValueTask.FromResult(element);
        }
    }
}