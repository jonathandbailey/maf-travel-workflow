using Infrastructure.Dto;
using MediatR;
using Travel.Application.Api.Infrastructure;

namespace Travel.Application.Api.Application.Queries;

public record GetFlightSearchQuery(Guid UserId, Guid Id) : IRequest<FlightSearchDto>;

public class GetFlightSearchHandler(IFlightRepository flightSearchRepository) : IRequestHandler<GetFlightSearchQuery, FlightSearchDto>
{
    public async Task<FlightSearchDto> Handle(GetFlightSearchQuery request, CancellationToken cancellationToken)
    {
        var flightSearch = await flightSearchRepository.GetFlightSearch(request.UserId, request.Id);
       
        return flightSearch;
    }
}