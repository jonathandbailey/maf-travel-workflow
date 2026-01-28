using Microsoft.Agents.AI;
using System.Diagnostics;
using Travel.Application.Api.Dto;

namespace Travel.Application.Api.Observability;

public static class TravelApiTelemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Travel.Application.Api", "1.0.0");

    public static Activity? CreateSession(Guid userId)
    {
        var tags = new ActivityTagsCollection
        {
            { "travel.user.id", userId }
        };

        return Source.StartActivity($"create_session", ActivityKind.Internal, null, tags);
    }

    public static void AddSession(this Activity activity, SessionDto sessionDto)
    {
        activity.SetTag("travel.session.id", sessionDto.ThreadId);
        activity.SetTag("travel.plan.id", sessionDto.TravelPlanId);
    }

    public static Activity? UpdateTravelPlan(Guid userId)
    {
        var tags = new ActivityTagsCollection
        {
            { "travel.user.id", userId }
        };

        return Source.StartActivity($"update_travel_plan", ActivityKind.Internal, null, tags);
    }
}