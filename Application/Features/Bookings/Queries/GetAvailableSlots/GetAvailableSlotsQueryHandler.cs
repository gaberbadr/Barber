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
        private readonly TimeProvider _timeProvider;

        public GetAvailableSlotsQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<List<AvailableSlotDTO>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            // Verify barber
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "الحلاق ده مش موجود.");

            if (!barber.IsActive)
                return Error.Failure("barber.inactive", "الحلاق ده غير مفعل.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Validation("barber.not.barber", "المستخدم ده مش حلاق.");

            if (!barber.AcceptingBookings)
                return new List<AvailableSlotDTO>();

            // Get settings
            var settingsRepo = _unitOfWork.Repository<GlobalBookingSettings, int>();
            var settings = (await settingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return Error.Failure("settings.missing", "الإعدادات مش مظبوطة.");

            // Check date range
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
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
            var validBookingStatuses = new[] { BookingStatus.Confirmed, BookingStatus.Arrived, BookingStatus.DidNotArrive };
            var existingBookings = await bookingRepo.FindAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate == request.Date &&
                validBookingStatuses.Contains(b.Status));

            var bookedSlots = existingBookings
                .Select(b => (b.StartTime, b.EndTime))
                .ToList();

            // Generate slots
            var slots = new List<AvailableSlotDTO>();
            var duration = barber.BookingDurationMinutes;
            var current = effectiveOpening;
            var now = _timeProvider.GetLocalNow().DateTime;
            var isToday = request.Date == today;

            while (true)
            {
                var slotEnd = current.AddMinutes(duration);

                // If adding duration caused a wrap around
                if (slotEnd < current) 
                    break;

                // Break if the slot exceeds closing time
                if (slotEnd > effectiveClosing)
                    break;

                // Skip past slots for today
                if (isToday)
                {
                    var slotDateTime = request.Date.ToDateTime(current);
                    if (slotDateTime < now)
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