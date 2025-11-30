using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.Events;

public sealed class TravelWorkflowErrorEvent(string description, string message, string nodeName, Exception? exception = null) : WorkflowErrorEvent(exception)
{
    public string Message { get; } = message;
    public string NodeName { get; } = nodeName;

    public string Description { get; } = description;
    public Exception? Exception { get; } = exception;
}

public sealed class ArtifactStatusEvent(string status) : WorkflowEvent
{
    public string Status { get; } = status;
}

public sealed class ReasonActWorkflowCompleteEvent(string message) : WorkflowEvent(message)
{
    public string Message { get; } = message;
}