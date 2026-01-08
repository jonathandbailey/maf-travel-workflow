using Microsoft.Extensions.AI;

namespace Application.Workflows.Dto;

public class TravelWorkflowRequestDto(ChatMessage message)
{
    public ChatMessage Message { get; } = message;
}