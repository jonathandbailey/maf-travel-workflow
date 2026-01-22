using System.Text.Json;
using Infrastructure.Dto;
using Infrastructure.Interfaces;

namespace Travel.Application.Api.Services;

public class FlightService(IArtifactRepository artifactRepository) : IFlightService
{
    public async Task<FlightSearchResultDto> GetFlightSearch(Guid userId, Guid searchId)
    {
        return await artifactRepository.GetFlightSearch(searchId);
    }

    public async Task<Guid> SaveFlightSearch(FlightSearchResultDto flightSearch)
    {
        var id = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(flightSearch);
        await artifactRepository.SaveFlightSearchAsync(payload, id);

        return id;
    }
}

public interface IFlightService
{
    Task<FlightSearchResultDto> GetFlightSearch(Guid userId, Guid searchId);
    Task<Guid> SaveFlightSearch(FlightSearchResultDto flightSearch);
}