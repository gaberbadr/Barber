using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Queries.GetById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, ErrorOr<BookingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetBookingByIdQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<BookingDTO>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var booking = await bookingRepo.GetAsync(request.BookingId);

            if (booking == null)
                return Error.NotFound("booking.not.found", "الحجز ده مش موجود.");

            // Only the customer, the barber, or an admin can view the booking
            var requestingUser = await _userManager.FindByIdAsync(request.RequestingUserId);
            if (requestingUser == null)
                return Error.NotFound("user.not.found", "المستخدم ده مش موجود.");

            var isAdmin = await _userManager.IsInRoleAsync(requestingUser, "Admin");
            var isCustomer = booking.CustomerId == request.RequestingUserId;
            var isBarber = booking.BarberId == request.RequestingUserId;

            if (!isCustomer && !isBarber && !isAdmin)
                return Error.Forbidden("booking.access.denied", "مش مسموح لك تشوف الحجز ده.");

            var customer = await _userManager.FindByIdAsync(booking.CustomerId);
            var barber = await _userManager.FindByIdAsync(booking.BarberId);

            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = customer?.FullName ?? "";
            dto.BarberName = barber?.FullName ?? "";

            var itemRepo = _unitOfWork.Repository<BookingItem, int>();
            var items = await itemRepo.FindAsync(bi => bi.BookingId == booking.Id);
            dto.Items = _mapper.Map<List<BookingItemDTO>>(items.ToList());

            return dto;
        }
    }
}