using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;

namespace Travel.Application.Api.Application.Queries;

public record GetSessionQuery(Guid UserId, Guid SessionId) : IRequest<SessionDto>;

public class GetSessionHandler(ISessionRepository sessionRepository)
    : IRequestHandler<GetSessionQuery, SessionDto>
{
    public async Task<SessionDto> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.LoadAsync(request.UserId, request.SessionId);
        
        var dto = new SessionDto(session.ThreadId, session.TravelPlanId);
        return dto;
    }
}