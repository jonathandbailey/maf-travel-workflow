using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Application.Users;
using Application.Workflows.Dto;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public class ArtifactRepository(IAzureStorageRepository repository, ISessionContextAccessor sessionContextAccessor, IOptions<AzureStorageSeedSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(string artifact, string name)
    {
        await repository.UploadTextBlobAsync(GetArtifactFileName(name),
            settings.Value.ContainerName,
            artifact, ApplicationJsonContentType);
    }

    public async Task<FlightSearchResultDto> GetFlightPlanAsync()
    {
        var filename = GetArtifactFileName("flights");

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var flightPlan = JsonSerializer.Deserialize<FlightSearchResultDto>(response, SerializerOptions);

        return flightPlan ?? throw new InvalidOperationException($"Failed to deserialize flight plan from blob: {filename}");
    }

    public async Task<bool> FlightsExistsAsync()
    {
        var filename = GetArtifactFileName("flights");
        var exists = await repository.BlobExists(filename, settings.Value.ContainerName);
        
        return exists;
    }

    public async Task<HotelSearchResultDto> GetHotelPlanAsync()
    {
        var filename = GetArtifactFileName("hotels");

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var hotelPlan = JsonSerializer.Deserialize<HotelSearchResultDto>(response, SerializerOptions);

        return hotelPlan ?? throw new InvalidOperationException($"Failed to deserialize hotel plan from blob: {filename}");
    }



    private string GetArtifactFileName(string name)
    {
        var userId = sessionContextAccessor.Context.UserId;
        var sessionId = sessionContextAccessor.Context.SessionId;

        return $"{userId}/{sessionId}/artifacts/{name}.json";
    }
}