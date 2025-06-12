namespace Resources.API.Settings;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
}

public class AdminUserSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 