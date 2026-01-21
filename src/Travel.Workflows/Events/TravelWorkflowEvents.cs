using Microsoft.Agents.AI.Workflows;

namespace Travel.Workflows.Events;

public sealed class TravelWorkflowErrorEvent(string description, string message, string nodeName, Exception? exception = null) : WorkflowErrorEvent(exception)
{
    public string Message { get; } = message;
    public string NodeName { get; } = nodeName;

    public string Description { get; } = description;
    public Exception? Exception { get; } = exception;
}

public sealed class ArtifactStatusEvent(Guid id, string key, ArtifactStatus status) : WorkflowEvent
{
    public Guid Id { get; } = id;
    public string Key { get; } = key;

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

public class UserStreamingEvent(string message) : WorkflowEvent(message)
{
    public string Content { get; } = message;

    public bool EndOfStream { get; }
}

public class UserStreamingCompleteEvent : WorkflowEvent { }

public class TravelPlanUpdatedEvent : WorkflowEvent { }

public class WorkflowStatusEvent : WorkflowEvent
{
    public WorkflowStatusEvent(string message, string details, string source) : base(message)
    {
        Status = message;
        Details = details;
        Source = source;
    }

    public string Source { get; }
    
    public string Status { get; }

    public string Details { get; }
}