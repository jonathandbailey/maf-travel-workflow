using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Models;

namespace Travel.Application.Api.Application.Commands;

public record CreateSessionCommand(Guid UserId) : IRequest<SessionDto>;

public class CreateSessionCommandHandler(ISessionRepository sessionRepository, ITravelPlanRepository travelPlanRepository) : IRequestHandler<CreateSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var travelPlan = new TravelPlan();
        
        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);

        var session = new Session(travelPlan.Id);

        await sessionRepository.SaveAsync(request.UserId, session);
        return new SessionDto(session.ThreadId, session.TravelPlanId);
    }
}