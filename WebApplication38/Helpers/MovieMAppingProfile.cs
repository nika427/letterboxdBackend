using AutoMapper;
using WebApplication38.DTO;
using WebApplication38.Models;
using WebApplication38.Requests;

namespace WebApplication38.Helpers
{
    public class MovieMAppingProfile : Profile
    {
        public MovieMAppingProfile()
        {

            CreateMap<AddMovieRequest, Movie>();
            CreateMap<Movie, MovieDTO>();
        }
    }
}
