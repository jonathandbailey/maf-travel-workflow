using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Services;

public interface ITravelPlanService
{
    Task SaveAsync(TravelPlan state, Guid userId);
    Task<bool> ExistsAsync(Guid userId, Guid travelPlanId);
    Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId);
    Task<TravelPlanSummary> GetSummary(Guid userId, Guid travelPlanId);
    Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate, Guid userId, Guid travelPlanId);
    Task<TravelPlan> AddFlightSearchOption(FlightSearchResultDto option, Guid userId, Guid travelPlanId);
    Task<TravelPlan> SelectFlightOption(FlightSearchResultDto option, Guid userId, Guid travelPlanId);
    Task<FlightSearchResultDto> GetFlightOptionsAsync(Guid userId, Guid travelPlanId);
    Task<Guid> CreateTravelPlan(Guid userId);
    Task<TravelPlan> GetTravelPlan(Guid userId, Guid travelPlanId);
}

public class TravelPlanService(IAzureStorageRepository repository, IArtifactRepository artifactRepository, IOptions<AzureStorageSeedSettings> settings) : ITravelPlanService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<TravelPlan> AddFlightSearchOption(FlightSearchResultDto option, Guid userId, Guid travelPlanId)  
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        var payload = JsonSerializer.Serialize(option, SerializerOptions);

        var id = Guid.NewGuid();

        await artifactRepository.SaveAsync(payload, id.ToString());

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(id));

        await SaveAsync(travelPlan, userId);

        return travelPlan;
    }

    public async Task<FlightSearchResultDto> GetFlightOptionsAsync(Guid userId, Guid travelPlanId)
    {

        var travelPlan = await LoadAsync(userId, travelPlanId);

        var filename = GetArtifactFileName(travelPlan.FlightPlan.FlightOptions.First().Id.ToString());

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var flightPlan = JsonSerializer.Deserialize<FlightSearchResultDto>(response, SerializerOptions);

        return flightPlan ?? throw new InvalidOperationException($"Failed to deserialize flight plan from blob: {filename}");
    }

    public async Task<TravelPlan> SelectFlightOption(FlightSearchResultDto option, Guid userId, Guid travelPlanId)
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        var flightOption = option.DepartureFlightOptions.First();

        var mapped = MapFlightOption(flightOption);

        travelPlan.SelectFlightOption(mapped);

        await SaveAsync(travelPlan, userId);

        return travelPlan;
    }

    private FlightOption MapFlightOption(FlightOptionDto flightOption)
    {
        return new FlightOption
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = new FlightEndpoint
            {
                Airport = flightOption.Departure.Airport,
                Datetime = flightOption.Departure.Datetime
            },
            Arrival = new FlightEndpoint
            {
                Airport = flightOption.Arrival.Airport,
                Datetime = flightOption.Arrival.Datetime
            },
            Duration = flightOption.Duration,
            Price = new FlightPrice
            {
                Amount = flightOption.Price.Amount,
                Currency = flightOption.Price.Currency
            }
        };
    }

    public async Task<TravelPlanSummary> GetSummary(Guid userId, Guid travelPlanId)
    {
        var travelPlan = await LoadAsync(userId, travelPlanId);

        var summary = new TravelPlanSummary(travelPlan);
      
        return summary;
    }

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

    public async Task<Guid> CreateTravelPlan(Guid userId)
    {
        var travelPlan = new TravelPlan();
 
        await SaveAsync(travelPlan, userId);

        return travelPlan.Id;
    }

    public async Task<TravelPlan> GetTravelPlan(Guid userId, Guid travelPlanId)
    {
        return await LoadAsync(userId, travelPlanId);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid travelPlanId)
    {
        return await repository.BlobExists(GetStorageFileName(userId, travelPlanId), settings.Value.ContainerName);
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

    private string GetArtifactFileName(string name)
    {
        return $"/artifacts/{name}.json";
    }
}