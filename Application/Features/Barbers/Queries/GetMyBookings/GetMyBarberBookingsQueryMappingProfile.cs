using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Barbers.Queries.GetMyBookings
{
    public class GetMyBarberBookingsQueryMappingProfile : Profile
    {
        public GetMyBarberBookingsQueryMappingProfile()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CustomerName, opt => opt.Ignore())
                .ForMember(d => d.BarberName, opt => opt.Ignore())
                .ForMember(d => d.CouponCode, opt => opt.MapFrom(s => s.CouponCodeSnapshot))
                .ForMember(d => d.Items, opt => opt.Ignore());

            CreateMap<BookingItem, BookingItemDTO>()
                .ForMember(d => d.ServiceName, opt => opt.MapFrom(s => s.ServiceNameSnapshot));
        }
    }
}