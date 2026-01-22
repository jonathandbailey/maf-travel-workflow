using Microsoft.Extensions.AI;

namespace Travel.Workflows.Dto;

public class WorkflowRequest(ChatMessage message, Guid threadId)
{
    public ChatMessage Message { get; } = message;
    public Guid ThreadId { get; } = threadId;
}

public record UserRequest(string Message);