using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.Events;

public sealed class TravelWorkflowErrorEvent(string description, string message, string nodeName, Exception? exception = null) : WorkflowErrorEvent(exception)
{
    public string Message { get; } = message;
    public string NodeName { get; } = nodeName;

    public string Description { get; } = description;
    public Exception? Exception { get; } = exception;
}

public sealed class ArtifactStatusEvent(string key, ArtifactStatus status) : WorkflowEvent
{
    private string Key { get; } = key;

    private ArtifactStatus Status { get; } = status;

    public override string ToString()
    {
        return $"{Key}:{Status}";
    }
}

public enum ArtifactStatus
{
    Created,
    Error
}

public sealed class ReasonActWorkflowCompleteEvent(string message) : WorkflowEvent(message)
{
    public string Message { get; } = message;
}