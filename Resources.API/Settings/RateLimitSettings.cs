namespace Resources.API.Settings;

public class RateLimitSettings
{
    public int PermitLimit { get; set; } = 100;
    public int Window { get; set; } = 10;
    public int QueueLimit { get; set; } = 2;
} 