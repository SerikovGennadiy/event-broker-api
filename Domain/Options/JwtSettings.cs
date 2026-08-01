namespace Domain.Options;

public class JwtSettings
{
    public string Section { get; set; } = "JwtSettings";
    public string? ValidIssuer { get; set; }
    public string? ValidAudience { get; set; }
    public string? ExpiresMinutes { get; set; }
    public string? Secret { get; set; }
}
