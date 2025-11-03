using Microsoft.Agents.AI.Workflows;

namespace ConsoleApp.Workflows;

public class CheckpointStore
{
    private readonly Dictionary<Guid, CheckpointInfo> _checkpoints = new();

    public void Add(Guid sessionId, CheckpointInfo checkpoint)
    {
        _checkpoints[sessionId] = checkpoint;
    }

    public CheckpointInfo Get(Guid sessionId)
    {
        return _checkpoints[sessionId];
    }

    public bool HasCheckpoint(Guid sessionId)
    {
        return _checkpoints.ContainsKey(sessionId);
    }
}
