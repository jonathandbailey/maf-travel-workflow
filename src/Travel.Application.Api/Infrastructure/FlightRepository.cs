using System.Text.Json;
using Infrastructure.Dto;
using Infrastructure.Interfaces;

namespace Travel.Application.Api.Infrastructure;

public class FlightRepository(IArtifactRepository artifactRepository) : IFlightRepository
{
    public async Task<FlightSearchDto> GetFlightSearch(Guid userId, Guid searchId)
    {
        return await artifactRepository.GetFlightSearch(searchId);
    }

    public async Task<Guid> SaveFlightSearch(FlightSearchDto flightSearch)
    {
        var id = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(flightSearch);
        await artifactRepository.SaveFlightSearchAsync(payload, id);

        return id;
    }
}

public interface IFlightRepository
{
    Task<FlightSearchDto> GetFlightSearch(Guid userId, Guid searchId);
    Task<Guid> SaveFlightSearch(FlightSearchDto flightSearch);
}