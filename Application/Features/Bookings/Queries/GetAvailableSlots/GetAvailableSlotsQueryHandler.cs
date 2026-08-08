using Application.Features.Bookings.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, ErrorOr<List<AvailableSlotDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAvailableSlotsQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ErrorOr<List<AvailableSlotDTO>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            // Verify barber
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "Barber not found.");

            if (!barber.IsActive)
                return Error.Failure("barber.inactive", "This barber is inactive.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Validation("barber.not.barber", "User is not a barber.");

            if (!barber.AcceptingBookings)
                return new List<AvailableSlotDTO>();

            // Get settings
            var settingsRepo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await settingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return Error.Failure("settings.missing", "Settings not configured.");

            // Check date range
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (request.Date < today)
                return new List<AvailableSlotDTO>();

            var maxDate = today.AddDays(settings.MaximumBookingAdvanceDays);
            if (request.Date > maxDate)
                return new List<AvailableSlotDTO>();

            // Shop hours
            var shopHoursRepo = _unitOfWork.Repository<ShopWorkingHour, int>();
            var shopHours = await shopHoursRepo.FindFirstAsync(
                s => s.DayOfWeek == request.Date.DayOfWeek);
            if (shopHours == null || shopHours.IsClosed)
                return new List<AvailableSlotDTO>();

            // Barber hours
            var barberHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var barberHours = await barberHoursRepo.FindFirstAsync(
                b => b.BarberId == request.BarberId && b.DayOfWeek == request.Date.DayOfWeek);
            if (barberHours == null || barberHours.IsClosed)
                return new List<AvailableSlotDTO>();

            // Calculate effective hours
            var effectiveOpening = shopHours.OpeningTime > barberHours.OpeningTime
                ? shopHours.OpeningTime : barberHours.OpeningTime;
            var effectiveClosing = shopHours.ClosingTime < barberHours.ClosingTime
                ? shopHours.ClosingTime : barberHours.ClosingTime;

            if (effectiveOpening >= effectiveClosing)
                return new List<AvailableSlotDTO>();

            // Get existing confirmed bookings for this barber on this date
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var existingBookings = await bookingRepo.FindAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate == request.Date &&
                b.Status == BookingStatus.Confirmed);

            var bookedSlots = existingBookings
                .Select(b => (b.StartTime, b.EndTime))
                .ToList();

            // Generate slots
            var slots = new List<AvailableSlotDTO>();
            var duration = barber.BookingDurationMinutes;
            var current = effectiveOpening;
            var now = DateTime.UtcNow;
            var isToday = request.Date == today;

            while (current.AddMinutes(duration) <= effectiveClosing)
            {
                var slotEnd = current.AddMinutes(duration);

                // Skip past slots for today
                if (isToday)
                {
                    var slotDateTime = request.Date.ToDateTime(current);
                    if (slotDateTime <= now)
                    {
                        current = slotEnd;
                        continue;
                    }
                }

                // Check overlap with existing bookings
                var isBooked = bookedSlots.Any(b =>
                    b.StartTime < slotEnd && b.EndTime > current);

                if (!isBooked)
                {
                    slots.Add(new AvailableSlotDTO
                    {
                        StartTime = current,
                        EndTime = slotEnd
                    });
                }

                current = slotEnd;
            }

            return slots;
        }
    }
}