using System.Diagnostics;

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
}