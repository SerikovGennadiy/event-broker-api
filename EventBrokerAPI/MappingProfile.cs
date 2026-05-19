using AutoMapper;
using Entities.Domain.Models;
using Shared.DTO;

namespace EventBrokerAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EventDTO, Event>();
        CreateMap<Event, EventDTO>();
        CreateMap<BookingDTO, Booking>();
        CreateMap<Booking, BookingDTO>();
    }
}
