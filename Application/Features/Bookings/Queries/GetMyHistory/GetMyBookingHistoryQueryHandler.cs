using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Zero.Core.Specification;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Queries.GetMyHistory
{
    public class GetMyBookingHistoryQueryHandler : IRequestHandler<GetMyBookingHistoryQuery, ErrorOr<List<BookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetMyBookingHistoryQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<List<BookingDTO>>> Handle(GetMyBookingHistoryQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var bookings = await bookingRepo.FindAsync(b =>
                b.CustomerId == request.CustomerId);

            var bookingList = bookings
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = new List<BookingDTO>();
            foreach (var booking in bookingList)
            {
                var dto = _mapper.Map<BookingDTO>(booking);

                var customer = await _userManager.FindByIdAsync(booking.CustomerId);
                var barber = await _userManager.FindByIdAsync(booking.BarberId);
                dto.CustomerName = customer?.FullName ?? "";
                dto.BarberName = barber?.FullName ?? "";

                var itemRepo = _unitOfWork.Repository<BookingItem, int>();
                var items = await itemRepo.FindAsync(bi => bi.BookingId == booking.Id);
                dto.Items = _mapper.Map<List<BookingItemDTO>>(items.ToList());

                dtos.Add(dto);
            }

            return dtos;
        }
    }
}