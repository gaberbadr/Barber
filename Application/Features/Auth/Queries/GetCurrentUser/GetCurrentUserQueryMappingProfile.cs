using Application.Features.Auth.DTOs;
using AutoMapper;
using Domain.Entities;


namespace Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryMappingProfile : Profile
    {
        public GetCurrentUserQueryMappingProfile()
        {
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));
        }
    }
}