using AutoMapper;
using WebApplication38.DTO;
using WebApplication38.Models;
using WebApplication38.Requests;

namespace WebApplication38.Helpers;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<AddReviewRequest, Review>();
        CreateMap<Review, ReviewDTO>();
    }
}
