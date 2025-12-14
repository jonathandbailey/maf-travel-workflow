using Application.Workflows.Dto;

namespace Application.Interfaces;

public interface IArtifactRepository
{
    Task SaveAsync(string artifact, string name);
    Task<FlightSearchResultDto> GetFlightPlanAsync();
    Task<HotelSearchResultDto> GetHotelPlanAsync();
    Task<bool> FlightsExistsAsync();
}