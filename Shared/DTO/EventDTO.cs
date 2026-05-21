namespace Shared.DTO;

// TODO Id в DTO из-за локального хранилища
public record EventDTO(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt);

public record CreateEvent(string Title, string? Description, DateTime StartAt, DateTime? EndAt, int TotalSeats);
public record EventInfo(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndA, int TotalSeats, int AvailableSeats);