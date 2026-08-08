using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Bookings.Commands.Cancel
{
    public class CancelBookingCommandMappingProfile : Profile
    {
        public CancelBookingCommandMappingProfile()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CustomerName, opt => opt.Ignore())
                .ForMember(d => d.BarberName, opt => opt.Ignore())
                .ForMember(d => d.CouponCode, opt => opt.MapFrom(s => s.CouponCodeSnapshot))
                .ForMember(d => d.Items, opt => opt.Ignore());
        }
    }
}