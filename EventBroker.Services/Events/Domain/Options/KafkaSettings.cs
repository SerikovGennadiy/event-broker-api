namespace Events.Domain.Options;

public class KafkaSettings
{
    public string Section { get; set; } = "Kafka";
    public string BootstrapServers { get; set; } = null!;
    public string? GroupId { get; set; }
}
