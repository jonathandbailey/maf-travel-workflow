using System.Text.Json;

namespace Application.Dto;

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
    public string? SessionId { get; set; }
    public string SchemaVersion { get; set; } = "1.0";
}