namespace Travel.Application.Api.Models;

public class Session(Guid userId, Guid travelPlanId)
{
    public Guid UserId { get; } = userId;
    public Guid ThreadId { get; } = Guid.NewGuid();
    public Guid TravelPlanId { get; } = travelPlanId;
}