namespace Events.Domain.Options;

public class RedisSettings
{
    public string Section { get; set; } = "Redis";

    public string EndPoint { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int ConnectTimeout { get; set; }
    public int SyncTimeout { get; set; }
    public bool AbortOnConnectFail { get; set; }
}
