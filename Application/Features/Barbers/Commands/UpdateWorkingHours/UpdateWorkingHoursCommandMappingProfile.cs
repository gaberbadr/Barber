using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommandMappingProfile : Profile
    {
        public UpdateWorkingHoursCommandMappingProfile()
        {
            CreateMap<ApplicationUser, BarberDTO>()
                .ForMember(d => d.WorkingHours, opt => opt.Ignore());
        }
    }
}