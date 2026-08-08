using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Barbers.Queries.GetAll
{
    public class GetAllBarbersQueryMappingProfile : Profile
    {
        public GetAllBarbersQueryMappingProfile()
        {
            CreateMap<ApplicationUser, BarberDTO>()
                .ForMember(d => d.WorkingHours, opt => opt.Ignore());
        }
    }
}