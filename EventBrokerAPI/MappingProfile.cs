using AutoMapper;
using Entities.Domain.Models;
using Shared.DTO;

namespace EventBrokerAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // При маппинге DTO -> Entity игнорируем поля, которыми управляет агрегат/БД
        CreateMap<EventDTO, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableSeats, opt => opt.Ignore());

        CreateMap<Event, EventDTO>();
        CreateMap<Event, EventInfo>();

        // CreateEvent используется через фабрику Event.Create, но маппинг оставлен на случай использования
        CreateMap<CreateEvent, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableSeats, opt => opt.Ignore());

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
