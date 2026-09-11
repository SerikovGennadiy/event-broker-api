namespace Events.Application.Common.Messaging;

public class OutboxMessage
{
    public Guid TraceId { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime TimeStampAt { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}