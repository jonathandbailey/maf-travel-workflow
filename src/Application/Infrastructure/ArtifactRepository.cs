using Application.Workflows.ReWoo.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Infrastructure;

public class ArtifactRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(Guid sessionId, Guid userId, string artifact, string name)
    {
        await repository.UploadTextBlobAsync(GetCheckpointFileName(sessionId, userId, name),
            settings.Value.ContainerName,
            artifact, ApplicationJsonContentType);
    }

    public async Task<FlightSearchResultDto> GetFlightPlanAsync(Guid sessionId, Guid userId)
    {
        var filename = GetCheckpointFileName(sessionId, userId, "flights");

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var flightPlan = JsonSerializer.Deserialize<FlightSearchResultDto>(response, SerializerOptions);

        return flightPlan ?? throw new InvalidOperationException($"Failed to deserialize flight plan from blob: {filename}");
    }

    private static string GetCheckpointFileName(Guid sessionId, Guid userId, string name)
    {
        return $"{userId}/{sessionId}/artifacts/{name}.json";
    }
}

public interface IArtifactRepository
{
    Task SaveAsync(Guid sessionId, Guid userId, string artifact, string name);
    Task<FlightSearchResultDto> GetFlightPlanAsync(Guid sessionId, Guid userId);
}