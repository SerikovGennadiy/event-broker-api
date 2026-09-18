namespace Bookings.Domain.Options;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = null!;
    public string? GroupId { get; set; }
}
