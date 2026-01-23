using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record UpdateTravelPlanCommand(Guid UserId, Guid SessionId, TravelPlanUpdateDto TravelPlanUpdateDto) : IRequest;

public class UpdateTravelPlanHandler(ITravelPlanService travelPlanService, ISessionService sessionService) : IRequestHandler<UpdateTravelPlanCommand>
{
    public async Task Handle(UpdateTravelPlanCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionService.Get(request.UserId, request.SessionId);

        await travelPlanService.UpdateAsync(request.TravelPlanUpdateDto, request.UserId, session.TravelPlanId);
    }
}