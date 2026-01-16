using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;
using Travel.Workflows.Models;

namespace Travel.Application.Api.Services;

public interface ITravelPlanService
{
    Task SaveAsync(TravelPlan state);
    Task<bool> ExistsAsync();
    Task<TravelPlan> LoadAsync();
    Task<TravelPlanSummary> GetSummary();
    Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate);
    Task<TravelPlan> AddFlightSearchOption(FlightSearchResultDto option);
    Task<TravelPlan> SelectFlightOption(FlightSearchResultDto option);
    Task<FlightSearchResultDto> GetFlightOptionsAsync();
    Task CreateTravelPlan();
}

public class TravelPlanService(IAzureStorageRepository repository, IArtifactRepository artifactRepository, IOptions<AzureStorageSeedSettings> settings) : ITravelPlanService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<TravelPlan> AddFlightSearchOption(FlightSearchResultDto option)
    {
        var travelPlan = await LoadAsync();

        var payload = JsonSerializer.Serialize(option, SerializerOptions);

        var id = Guid.NewGuid();

        await artifactRepository.SaveAsync(payload, id.ToString());

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(id));

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

    public async Task<TravelPlan> SelectFlightOption(FlightSearchResultDto option)
    {
        var travelPlan = await LoadAsync();

        var flightOption = option.DepartureFlightOptions.First();

        var mapped = MapFlightOption(flightOption);

        travelPlan.SelectFlightOption(mapped);

        await SaveAsync(travelPlan);

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
        return $"/plans/travel-plan.json";
    }

    private string GetArtifactFileName(string name)
    {
        return $"/artifacts/{name}.json";
    }
}