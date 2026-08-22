using AutoMapper;
using Events.Application.Common.DTO;
using Events.Domain.Models;

namespace Application;

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
    }
}
