using MediatR;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record CreateTravelPlanCommand(Guid UserId) : IRequest<Guid>;

public class CreateTravelPlanCommandHandler(ITravelPlanService travelPlanService) : IRequestHandler<CreateTravelPlanCommand, Guid>
{
    public async Task<Guid> Handle(CreateTravelPlanCommand request, CancellationToken cancellationToken)
    {
        var id = await travelPlanService.CreateAsync(request.UserId);

        return id;
    }
}