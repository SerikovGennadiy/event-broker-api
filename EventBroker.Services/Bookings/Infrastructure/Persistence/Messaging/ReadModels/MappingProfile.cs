using AutoMapper;
using Bookings.Application.Common.DTO;
using Bookings.Infrastructure.Persistence.Messaging.ReadModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EventReadDTO, EventRead>().ReverseMap();
    }
}
