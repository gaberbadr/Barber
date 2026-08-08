using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Barbers.Queries.GetById
{
    public class GetBarberByIdQueryMappingProfile : Profile
    {
        public GetBarberByIdQueryMappingProfile()
        {
            CreateMap<ApplicationUser, BarberDTO>()
                .ForMember(d => d.WorkingHours, opt => opt.Ignore());
        }
    }
}