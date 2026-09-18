using AutoMapper;
using Bookings.Application.Common.DTO;
using Bookings.Domain.Models;
namespace Bookings.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Booking: DTO <-> DTO mapping (entity имеет приватные сеттеры, маппинг на сущность редко нужен)
        CreateMap<Booking, BookingDTO>();
        CreateMap<BookingDTO, Booking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EventId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore());

    }
}
