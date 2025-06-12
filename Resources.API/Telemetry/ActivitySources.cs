using System.Diagnostics;

namespace Resources.API.Telemetry;

public static class ActivitySources
{
    public const string ServiceName = "Resources.API";
    public static readonly ActivitySource ResourcesApi = new(ServiceName);
} 