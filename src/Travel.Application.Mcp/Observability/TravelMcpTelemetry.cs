using System.Diagnostics;

namespace Travel.Application.Mcp.Observability;

public static class TravelMcpTelemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Travel.Application.Mcp", "1.0.0");

    public static Activity? SearchFlights()
    {
        return Source.StartActivity($"search_flights", ActivityKind.Internal, null);
    }
}