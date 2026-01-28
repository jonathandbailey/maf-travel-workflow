using MediatR;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Travel.Application.Api.Dto;
using Travel.Application.Application.Commands;
using Travel.Application.Application.Queries;
using Travel.Application.Mcp.Observability;

namespace Travel.Application.Mcp.Tools;

[McpServerToolType]
public class FlightTools(IMediator mediator)
{
    [McpServerTool]
    [Description("Get a flight search by ID")]
    public async Task<FlightSearchResultDto> GetFlightSearch(
       [Description("The Id of the flight search")] Guid id)
    {
        var result = await mediator.Send(new GetFlightSearchQuery(id));
        return result;
    }

    [McpServerTool]
    [Description("Search for flights")]
    public async Task<FlightSearchResponseDto> SearchFlights(
       [Description("The flight search criteria")] FlightSearchDto flightSearch)
    {
        using var activity = TravelMcpTelemetry.SearchFlights();
        
        var flightSearchResultDto = await mediator.Send(new SearchFlightsCommand(flightSearch.Origin, flightSearch.Destination, flightSearch.DepartureDate, flightSearch.ReturnDate));

        return new FlightSearchResponseDto(flightSearchResultDto.Id);
    }
}