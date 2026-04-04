namespace Shared.DTO;

// TODO Id в DTO из-за локального хранилища
public record EventDTO(string Title, string Description, DateTime StartAt, DateTime EndAt);
