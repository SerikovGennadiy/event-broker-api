using AutoMapper;
using Users.Application.Common.DTO;
using Users.Domain.Models;

namespace Users.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDTO, User>();
        CreateMap<User, UserDTO>();
    }
}
