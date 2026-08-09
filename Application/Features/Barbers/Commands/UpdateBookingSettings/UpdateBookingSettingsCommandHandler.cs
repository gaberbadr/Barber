using Application.Features.Barbers.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Commands.UpdateBookingSettings
{
    public class UpdateBookingSettingsCommandHandler : IRequestHandler<UpdateBookingSettingsCommand, ErrorOr<BarberDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        public UpdateBookingSettingsCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TimeProvider timeProvider)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<BarberDTO>> Handle(UpdateBookingSettingsCommand request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "Barber not found.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Forbidden("barber.not.barber", "User is not a barber.");

            // If changing duration, check for affected future bookings
            if (barber.BookingDurationMinutes != request.BookingDurationMinutes)
            {
                var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
                var bookingRepo = _unitOfWork.Repository<Booking, int>();
                var futureBookings = await bookingRepo.FindAsync(b =>
                    b.BarberId == request.BarberId &&
                    b.BookingDate >= today &&
                    b.Status == BookingStatus.Confirmed);

                if (futureBookings.Any())
                    return Error.Conflict("barber.duration.has.bookings",
                        "Cannot change booking duration while you have future confirmed bookings. " +
                        "Please wait until they are completed or cancel them first.");
            }

            barber.BookingDurationMinutes = request.BookingDurationMinutes;
            barber.AcceptingBookings = request.AcceptingBookings;
            barber.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

            await _userManager.UpdateAsync(barber);

            var dto = _mapper.Map<BarberDTO>(barber);

            var workingHoursRepo = _unitOfWork.Repository<BarberWorkingHour, int>();
            var workingHours = await workingHoursRepo.FindAsync(w => w.BarberId == barber.Id);
            dto.WorkingHours = workingHours.Select(w => new BarberWorkingHourDTO
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                DayName = w.DayOfWeek.ToString(),
                OpeningTime = w.OpeningTime,
                ClosingTime = w.ClosingTime,
                IsClosed = w.IsClosed
            }).OrderBy(w => w.DayOfWeek).ToList();

            return dto;
        }
    }
}