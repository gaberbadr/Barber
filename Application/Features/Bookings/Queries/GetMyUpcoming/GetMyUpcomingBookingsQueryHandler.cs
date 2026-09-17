using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Queries.GetMyUpcoming
{
    public class GetMyUpcomingBookingsQueryHandler : IRequestHandler<GetMyUpcomingBookingsQuery, ErrorOr<List<BookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        public GetMyUpcomingBookingsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<List<BookingDTO>>> Handle(GetMyUpcomingBookingsQuery request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var bookingsQuery = await bookingRepo.GetIQueryable()
                .Where(b =>
                    b.CustomerId == request.CustomerId &&
                    b.Status == BookingStatus.Confirmed &&
                    b.BookingDate >= today)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Select(b => new
                {
                    Booking = b,
                    CustomerName = b.Customer != null ? b.Customer.FullName : null,
                    CustomerPhone = b.Customer != null ? b.Customer.PhoneNumber : null,
                    BarberName = b.Barber != null ? b.Barber.FullName : null,
                    Items = b.BookingItems.Select(bi => new BookingItemDTO
                    {
                        Id = bi.Id,
                        ServiceId = bi.ServiceId,
                        ServiceName = bi.ServiceNameSnapshot,
                        UnitPrice = bi.UnitPrice,
                        Quantity = bi.Quantity,
                        TotalPrice = bi.TotalPrice
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = new List<BookingDTO>();
            foreach (var item in bookingsQuery)
            {
                var dto = _mapper.Map<BookingDTO>(item.Booking);
                dto.CustomerName = item.CustomerName ?? "";
                dto.CustomerPhone = item.CustomerPhone;
                dto.BarberName = item.BarberName ?? "";
                dto.Items = item.Items;
                dtos.Add(dto);
            }

            return dtos;
        }
    }
}