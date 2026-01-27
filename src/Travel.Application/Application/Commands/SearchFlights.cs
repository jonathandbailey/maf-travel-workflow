using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Infrastructure.Mappers;
using Travel.Application.Services;

namespace Travel.Application.Application.Commands;

public record SearchFlightsCommand(
    string Origin,
    string Destination,
    DateTimeOffset DepartureDate,
    DateTimeOffset ReturnDate) : IRequest<FlightSearchResultDto>;

public class SearchFlightsCommandHandler(IFlightSearchService flightSearchService, IFlightRepository flightRepository) : IRequestHandler<SearchFlightsCommand, FlightSearchResultDto>
{
    public async Task<FlightSearchResultDto> Handle(SearchFlightsCommand request, CancellationToken cancellationToken)
    {
        var result = await flightSearchService.SearchFlights(
            request.Origin,
            request.Destination,
            request.DepartureDate,
            request.ReturnDate);

        await flightRepository.SaveFlightSearch(result);

        return result.ToDto("Flights");
    }
}