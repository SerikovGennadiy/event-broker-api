using Entities.Domain.Models;

namespace Shared.DTO;

public record BookingDTO(Guid Id, Guid EventId, BookingStatus Status, DateTime CreatedAt, DateTime? ProcessedAt);
//public record CreateBookingDTO (Guid EventId);
//public record UpdateBookingDTO(Guid Id);