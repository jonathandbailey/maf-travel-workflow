using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Dto;

public class TravelWorkflowRequestDto(ChatMessage message, string threadId)
{
    public ChatMessage Message { get; } = message;
    public string ThreadId { get; } = threadId;
}

public class WorkflowRequest
{
    public Meta Meta { get; set; } = new();
    public Dictionary<string, JsonElement> Payload { get; set; } = new();
}

public sealed class Meta
{
    public string RawUserMessage { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public float? Confidence { get; set; }
    public string ThreadId { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = "1.0";
}

public record UserRequest(string Message);