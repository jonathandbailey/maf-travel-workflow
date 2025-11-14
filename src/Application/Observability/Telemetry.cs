using System.Diagnostics;

namespace Application.Observability;

public static class Telemetry
{
    private static readonly ActivitySource Source = new ActivitySource("application.workflows.react", "1.0.0");

    public static Activity? Start(string name)
    {
        return Source.StartActivity(name);
    }
}