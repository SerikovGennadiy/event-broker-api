using System.ComponentModel.DataAnnotations;

namespace Events.Application.Common.Messaging;

public class InboxMessage
{
    [Key]
    public Guid Id { get; set; }
    public Guid TraceId { get; set; } 
    public string Type { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string? Content { get; set; }
    public int ReadAttempts { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}
