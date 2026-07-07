using Domain.Models;

namespace Application.Shared.DTO;

public record BookingDTO(Guid Id, Guid EventId, BookingStatus Status, DateTime CreatedAt, DateTime? ProcessedAt);
public record UpdateBookingDTO(DateTime ProcessedAt, BookingStatus Status);