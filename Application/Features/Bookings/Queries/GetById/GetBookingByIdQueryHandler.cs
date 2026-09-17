using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var bookingData = await bookingRepo.GetIQueryable()
                .Where(b => b.Id == request.BookingId)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (bookingData == null || bookingData.Booking == null)
                return Error.NotFound("booking.not.found", "الحجز ده مش موجود.");

            var booking = bookingData.Booking;

            // Only the customer, the barber, or an admin can view the booking
            var requestingUser = await _userManager.FindByIdAsync(request.RequestingUserId);
            if (requestingUser == null)
                return Error.NotFound("user.not.found", "المستخدم ده مش موجود.");

            var isAdmin = await _userManager.IsInRoleAsync(requestingUser, "Admin");
            var isCustomer = booking.CustomerId == request.RequestingUserId;
            var isBarber = booking.BarberId == request.RequestingUserId;

            if (!isCustomer && !isBarber && !isAdmin)
                return Error.Forbidden("booking.access.denied", "مش مسموح لك تشوف الحجز ده.");

            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = bookingData.CustomerName ?? "";
            dto.CustomerPhone = bookingData.CustomerPhone;
            dto.BarberName = bookingData.BarberName ?? "";
            dto.Items = bookingData.Items;

            return dto;
        }
    }
}
