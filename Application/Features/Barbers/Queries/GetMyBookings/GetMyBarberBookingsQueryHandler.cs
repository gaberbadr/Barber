using Application.Common.Pagination;
using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Queries.GetMyBookings
{
    public class GetMyBarberBookingsQueryHandler : IRequestHandler<GetMyBarberBookingsQuery, ErrorOr<PaginationResponse<BookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetMyBarberBookingsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<PaginationResponse<BookingDTO>>> Handle(GetMyBarberBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var bookings = await bookingRepo.FindAsync(b =>
                b.BarberId == request.BarberId &&
                (!request.FromDate.HasValue || b.BookingDate >= request.FromDate.Value) &&
                (!request.ToDate.HasValue || b.BookingDate <= request.ToDate.Value));

            var totalCount = bookings.Count();

            var bookingList = bookings
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
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

            return new PaginationResponse<BookingDTO>(request.PageSize, request.PageIndex, totalCount, dtos);
        }
    }
}