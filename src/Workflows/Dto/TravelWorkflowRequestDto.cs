using Microsoft.Extensions.AI;

namespace Workflows.Dto;

public class TravelWorkflowRequestDto(ChatMessage message, string threadId)
{
    public ChatMessage Message { get; } = message;
    public string ThreadId { get; } = threadId;
}