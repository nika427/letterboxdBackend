using AutoMapper;
using WebApplication38.DTO;
using WebApplication38.Models;
using WebApplication38.Requests;

namespace WebApplication38.Helpers;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<AddUserRequest, User>();
        CreateMap<User, UserDTO>();
    }
}
