using Infrastructure.Dto;

namespace Infrastructure.Interfaces;

public interface IArtifactRepository
{
    Task SaveAsync(string artifact, string name);
    Task<FlightSearchResultDto> GetFlightPlanAsync();
    Task<bool> FlightsExistsAsync();
    Task SaveFlightSearchAsync(string artifact, Guid id);
}