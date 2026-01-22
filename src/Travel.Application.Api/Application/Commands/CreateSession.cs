using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record CreateSessionCommand(Guid UserId, Guid TravelPlanId) : IRequest<SessionDto>;

public class CreateSessionCommandHandler(ISessionService sessionService) : IRequestHandler<CreateSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionService.Create(request.UserId, request.TravelPlanId);
        return new SessionDto(session.ThreadId, session.TravelPlanId);
    }
}