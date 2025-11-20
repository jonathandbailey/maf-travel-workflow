using System.Text.Json.Serialization;

namespace Application.Workflows.ReWoo.Dto;

public class OrchestrationRequestDto
{
    [JsonPropertyName("route")]
    public string Route { get; set; } = string.Empty;

    [JsonPropertyName("tasks")]
    public List<OrchestratorWorkerTaskDto> Tasks { get; set; } = new();
}

public class OrchestratorWorkerTaskDto
{
    [JsonPropertyName("worker")]
    public string Worker { get; set; } = string.Empty;

    [JsonPropertyName("inputs")]
    public Dictionary<string, string> Inputs { get; set; } = new();

    [JsonPropertyName("artifact_key")]
    public string ArtifactKey { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Worker: {Worker}, Inputs: {string.Join(", ", Inputs)}, ArtifactKey: {ArtifactKey}";
    }
}

