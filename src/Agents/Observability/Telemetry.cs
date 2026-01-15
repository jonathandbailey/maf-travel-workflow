using System.Diagnostics;

namespace Agents.Observability;

public static class Telemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Agents", "1.0.0");

    public static Activity? Start(string name)
    {
        return Source.StartActivity(name);
    }
}