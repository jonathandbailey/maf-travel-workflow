using System.Text.Json;
using Infrastructure.Interfaces;
using Travel.Application.Api.Dto;

namespace Travel.Application.Api.Infrastructure;

public class FlightRepository(IArtifactRepository artifactRepository) : IFlightRepository
{
    public async Task<FlightSearchDto> GetFlightSearch(Guid userId, Guid searchId)
    {
        var paylod = await artifactRepository.LoadAsync(searchId, "flights");

        var dto = JsonSerializer.Deserialize<FlightSearchDto>(paylod);

        if (dto == null)
        {
            throw new ArgumentException("Failed to deserialize flight search");
        }

        return dto;
    }

    public async Task<Guid> SaveFlightSearch(FlightSearchDto flightSearch)
    {
        var id = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(flightSearch);
        await artifactRepository.SaveAsync(payload, id, "flights");

        return id;
    }
}

public interface IFlightRepository
{
    Task<FlightSearchDto> GetFlightSearch(Guid userId, Guid searchId);
    Task<Guid> SaveFlightSearch(FlightSearchDto flightSearch);
}