namespace Entities.Models;

public class Event
{
    public Guid Id { get; init; }
    public required string Title { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    
    public string? Description { get; set; }
}
