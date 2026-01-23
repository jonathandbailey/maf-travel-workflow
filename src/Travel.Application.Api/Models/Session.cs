namespace Travel.Application.Api.Models;

public class Session(Guid travelPlanId)
{
    public Guid ThreadId { get; } = Guid.NewGuid();
    public Guid TravelPlanId { get; } = travelPlanId;
}