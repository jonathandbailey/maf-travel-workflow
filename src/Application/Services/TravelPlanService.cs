using Application.Users;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Application.Models;
using Application.Workflows.Dto;
using Infrastructure.Settings;
using Application.Models.Flights;

namespace Application.Services;

public interface ITravelPlanService
{
    Task SaveAsync(TravelPlan state);
    Task<bool> ExistsAsync();
    Task<TravelPlan> LoadAsync();
    Task<TravelPlanSummary> GetSummary();
    Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate);
    Task<TravelPlan> AddFlightSearchOption(FlightOptionSearch option);
    Task<TravelPlan> SelectFlightOption(FlightOption flightOption);
    Task<FlightSearchResultDto> GetFlightOptionsAsync();
    Task CreateTravelPlan();
}

public class TravelPlanService(IAzureStorageRepository repository, ISessionContextAccessor sessionContextAccessor, IOptions<AzureStorageSeedSettings> settings) : ITravelPlanService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = false,
        ReadCommentHandling = JsonCommentHandling.Disallow
    };

    public async Task<TravelPlan> AddFlightSearchOption(FlightOptionSearch option)
    {
        var travelPlan = await LoadAsync();

        travelPlan.AddFlightSearchOption(option);

        await SaveAsync(travelPlan);

        return travelPlan;
    }

    public async Task<FlightSearchResultDto> GetFlightOptionsAsync()
    {

        var travelPlan = await LoadAsync();
        
        var filename = GetArtifactFileName(travelPlan.FlightPlan.FlightOptions.First().Id.ToString());

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var flightPlan = JsonSerializer.Deserialize<FlightSearchResultDto>(response, SerializerOptions);

        return flightPlan ?? throw new InvalidOperationException($"Failed to deserialize flight plan from blob: {filename}");
    }

    public async Task<TravelPlan> SelectFlightOption(FlightOption flightOption)
    {
        var travelPlan = await LoadAsync();

        travelPlan.SelectFlightOption(flightOption);

        await SaveAsync(travelPlan);

        return travelPlan;
    }

    public async Task<TravelPlanSummary> GetSummary()
    {
        var travelPlan = await LoadAsync();

        var summary = new TravelPlanSummary(travelPlan);
      
        return summary;
    }

    public async Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate)
    {
        var travelPlan = await LoadAsync();

        travelPlan.InProgress();

        if (!string.IsNullOrEmpty(messageTravelPlanUpdate.Origin))
            travelPlan.SetOrigin(messageTravelPlanUpdate.Origin);

        if (!string.IsNullOrEmpty(messageTravelPlanUpdate.Destination))
            travelPlan.SetDestination(messageTravelPlanUpdate.Destination);

        if (messageTravelPlanUpdate.StartDate.HasValue)
            travelPlan.SetStartDate(messageTravelPlanUpdate.StartDate.Value);

        if (messageTravelPlanUpdate.EndDate.HasValue)
            travelPlan.SetEndDate(messageTravelPlanUpdate.EndDate.Value);

        await SaveAsync(travelPlan);
    }

    public async Task SaveAsync(TravelPlan state)
    {
        var serializedConversation = JsonSerializer.Serialize(state, SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetStorageFileName(),
            settings.Value.ContainerName,
            serializedConversation,
            ApplicationJsonContentType);
    }

    public async Task CreateTravelPlan()
    {
        if (!await repository.BlobExists(GetStorageFileName(), settings.Value.ContainerName))
        {
            var travelPlan = new TravelPlan();

            await SaveAsync(travelPlan);
        }
    }

    public async Task<bool> ExistsAsync()
    {
        return await repository.BlobExists(GetStorageFileName(), settings.Value.ContainerName);
    }

    public async Task<TravelPlan> LoadAsync()
    {
        var blob = await repository.DownloadTextBlobAsync(GetStorageFileName(), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<TravelPlan>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Travel Plan for session.");

        return stateDto;
    }


    private string GetStorageFileName()
    {
        var userId = sessionContextAccessor.Context.UserId;
        var sessionId = sessionContextAccessor.Context.SessionId;

        return $"{userId}/{sessionId}/plans/travel-plan.json";
    }

    private string GetArtifactFileName(string name)
    {
        var userId = sessionContextAccessor.Context.UserId;
        var sessionId = sessionContextAccessor.Context.SessionId;

        return $"{userId}/{sessionId}/artifacts/{name}.json";
    }
}