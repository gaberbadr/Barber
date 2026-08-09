using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Commands.Cancel
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ErrorOr<BookingDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        public CancelBookingCommandHandler(
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

        public async Task<ErrorOr<BookingDTO>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var booking = await bookingRepo.GetAsync(request.BookingId);

            if (booking == null)
                return Error.NotFound("booking.not.found", "Booking not found.");

            // Verify the user cancelling is the customer or an admin
            var canceller = await _userManager.FindByIdAsync(request.CancelledByUserId);
            if (canceller == null)
                return Error.NotFound("booking.canceller.not.found", "User not found.");

            var isAdmin = await _userManager.IsInRoleAsync(canceller, "Admin");
            var isOwner = booking.CustomerId == request.CancelledByUserId;

            if (!isOwner && !isAdmin)
                return Error.Forbidden("booking.not.authorized", "You can only cancel your own bookings.");

            if (booking.Status == BookingStatus.Cancelled)
                return Error.Validation("booking.already.cancelled", "This booking is already cancelled.");

            // Get global settings
            var settingsRepo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await settingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return Error.Failure("booking.settings.missing", "Booking settings not configured.");

            // Check cancellation window (16 hours before appointment)
            var appointmentDateTime = booking.BookingDate.ToDateTime(booking.StartTime);
            var cancellationDeadline = appointmentDateTime.AddHours(-settings.CancellationWindowHours);
            var localNow = _timeProvider.GetLocalNow().DateTime;

            if (localNow > cancellationDeadline && !isAdmin)
                return Error.Validation("booking.cancellation.window.passed",
                    $"Bookings can only be cancelled at least {settings.CancellationWindowHours} hours before the appointment.");

            var utcNow = _timeProvider.GetUtcNow().DateTime;
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = utcNow;
            booking.CancelledBy = request.CancelledByUserId;
            booking.UpdatedAt = utcNow;

            bookingRepo.Update(booking);
            await _unitOfWork.CompleteAsync();

            var customer = await _userManager.FindByIdAsync(booking.CustomerId);
            var barber = await _userManager.FindByIdAsync(booking.BarberId);

            var dto = _mapper.Map<BookingDTO>(booking);
            dto.CustomerName = customer?.FullName ?? "";
            dto.BarberName = barber?.FullName ?? "";

            return dto;
        }
    }
}