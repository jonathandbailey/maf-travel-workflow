using System.Diagnostics;

namespace Application.Observability;

public static class Telemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Application", "1.0.0");

    public static Activity? StarActivity(string name)
    {
        return Source.StartActivity(name);
    }
}