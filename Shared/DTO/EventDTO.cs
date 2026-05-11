namespace Shared.DTO;

// TODO Id в DTO из-за локального хранилища
public record EventDTO(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt);
