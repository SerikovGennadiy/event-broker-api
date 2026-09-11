namespace Bookings.Application.Common.Messaging;

public class InboxMessage
{
    public Guid TraceId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Content { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int ReadAttempts { get; set; }
    public string? Error { get; set; }
}
