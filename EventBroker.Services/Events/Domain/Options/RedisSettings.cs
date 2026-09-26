namespace Events.Domain.Options;

public class RedisSettings
{
    public const string Section = "Redis";

    public string EndPoint { get; set; } = null!;
    public string Password { get; set; } = null!;

    #region TTL кешей по заданию
    public int EventTtlMinutes { get; set; }
    public int TopEventsTtlMinutes { get; set; }
    #endregion
    public int ConnectTimeout { get; set; }
    public int SyncTimeout { get; set; }
    public bool AbortOnConnectFail { get; set; }
}
