using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Barbers.Commands.UpdateBookingSettings
{
    public class UpdateBookingSettingsCommandMappingProfile : Profile
    {
        public UpdateBookingSettingsCommandMappingProfile()
        {
            CreateMap<ApplicationUser, BarberDTO>()
                .ForMember(d => d.WorkingHours, opt => opt.Ignore());
        }
    }
}