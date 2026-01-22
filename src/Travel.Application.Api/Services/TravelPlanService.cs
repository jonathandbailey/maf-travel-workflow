using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Services;



public class TravelPlanService(IAzureStorageRepository repository, IArtifactRepository artifactRepository, IOptions<AzureStorageSeedSettings> settings) : ITravelPlanService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate, Guid userId, Guid travelPlanId)
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        travelPlan.InProgress();

        if (!string.IsNullOrEmpty(messageTravelPlanUpdate.Origin))
            travelPlan.SetOrigin(messageTravelPlanUpdate.Origin);

        if (!string.IsNullOrEmpty(messageTravelPlanUpdate.Destination))
            travelPlan.SetDestination(messageTravelPlanUpdate.Destination);

        if (messageTravelPlanUpdate.StartDate.HasValue)
            travelPlan.SetStartDate(messageTravelPlanUpdate.StartDate.Value);

        if (messageTravelPlanUpdate.EndDate.HasValue)
            travelPlan.SetEndDate(messageTravelPlanUpdate.EndDate.Value);

        await SaveAsync(travelPlan, userId);
    }

    public async Task SaveAsync(TravelPlan travelPlan, Guid userId)
    {
        var serializedConversation = JsonSerializer.Serialize(travelPlan, SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetStorageFileName(userId, travelPlan.Id),
            settings.Value.ContainerName,
            serializedConversation,
            ApplicationJsonContentType);
    }

    public async Task UpdateFlightSearchOption(Guid userId, Guid travelPlanId, Guid searchId)
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(searchId));

        await SaveAsync(travelPlan, userId);
    }

    public async Task SaveFlightOption(Guid userId, Guid travelPlanId, FlightOption flightOption)
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        travelPlan.SelectFlightOption(flightOption);

        await SaveAsync(travelPlan, userId);
    }

    public async Task<Guid> CreateAsync(Guid userId)
    {
        var travelPlan = new TravelPlan();
 
        await SaveAsync(travelPlan, userId);

        return travelPlan.Id;
    }

    public async Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId)
    {
        var blob = await repository.DownloadTextBlobAsync(GetStorageFileName(userId, travelPlanId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<TravelPlan>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Travel Plan for session.");

        return stateDto;
    }


    private string GetStorageFileName(Guid userId, Guid travelPlanId)
    {
        return $"{userId}/plans/travel-plan-{travelPlanId}.json";
    }
}

public interface ITravelPlanService
{
    Task SaveAsync(TravelPlan state, Guid userId);
    Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId);
    Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate, Guid userId, Guid travelPlanId);
    Task<Guid> CreateAsync(Guid userId);
    Task UpdateFlightSearchOption(Guid userId, Guid travelPlanId, Guid searchId);
    Task SaveFlightOption(Guid userId, Guid travelPlanId, FlightOption flightOption);
}