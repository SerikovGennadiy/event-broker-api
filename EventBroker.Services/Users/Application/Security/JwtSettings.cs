namespace Domain.Options;

public class JwtSettings
{
    public string Section { get; set; } = "JwtSettings";
    public int ExpiresMinutes { get; set; } = 10;

    public string? ValidIssuer { get; set; }
    public string? ValidAudience { get; set; }
    public string? Secret { get; set; }
}
