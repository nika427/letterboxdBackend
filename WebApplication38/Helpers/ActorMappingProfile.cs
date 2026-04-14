using AutoMapper;
using WebApplication38.DTO;
using WebApplication38.Models;
using WebApplication38.Requests;
namespace WebApplication38.Helpers
{
    public class ActorMappingProfile : Profile
    {
        public ActorMappingProfile()
        {
           
            CreateMap<AddActorRequest, Actor>();
            CreateMap<Actor, ActorDTO>();

            
        }
    }
}
