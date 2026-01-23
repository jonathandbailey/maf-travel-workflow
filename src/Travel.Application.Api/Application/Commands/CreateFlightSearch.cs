using Infrastructure.Dto;
using MediatR;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Application.Commands;

public record CreateFlightSearchCommand(Guid UserId, Guid SessionId, FlightSearchDto FlightSearch) : IRequest<Guid>;

public class CreateFlightSearchCommandHandler(ITravelPlanRepository travelPlanRepository, IFlightRepository flightRepository, ISessionRepository sessionRepository) :
    IRequestHandler<CreateFlightSearchCommand, Guid>
{
    public async Task<Guid> Handle(CreateFlightSearchCommand request, CancellationToken cancellationToken)
    {
        var id = await flightRepository.SaveFlightSearch(request.FlightSearch);

        var session = await sessionRepository.LoadAsync(request.UserId, request.SessionId);

        var travelPlan = await travelPlanRepository.LoadAsync(request.UserId, session.TravelPlanId);

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(id));

        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);

        return id;
    }
}