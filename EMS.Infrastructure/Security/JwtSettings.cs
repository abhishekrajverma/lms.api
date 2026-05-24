namespace EMS.Infrastructure.Security;

/// <summary>
/// JWT Settings configuration
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public string Issuer { get; set; } = "ems-api";
    public string Audience { get; set; } = "ems-clients";
}
