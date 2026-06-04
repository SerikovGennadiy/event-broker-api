namespace Shared.DTO;


public record EventDTO(string Title,
                       string? Description,
                       DateTime StartAt,
                       DateTime EndAt,
                       int TotalSeats);

public record CreateEvent(string Title,
                          string? Description,
                          DateTime StartAt,
                          DateTime EndAt,
                          int TotalSeats)
                : EventDTO(Title, Description, StartAt, EndAt, TotalSeats);

public record EventInfo(Guid Id,
                        string Title,
                        string? Description,
                        DateTime StartAt,
                        DateTime EndAt,
                        int TotalSeats,
                        int AvailableSeats)
                : EventDTO(Title, Description, StartAt, EndAt, TotalSeats);