using Domain.Entities;
using Domain.Repositories;
using Domain.Enums;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Barbers.Commands.RemoveBarber
{
    public class RemoveBarberCommandHandler : IRequestHandler<RemoveBarberCommand, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveBarberCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> Handle(RemoveBarberCommand request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "Barber not found.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Forbidden("barber.not.barber", "User is not a barber.");

            // Check for active/upcoming bookings
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var upcomingBookings = await bookingRepo.FindAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate >= DateOnly.FromDateTime(DateTime.UtcNow.Date) &&
                b.Status == BookingStatus.Confirmed);

            if (upcomingBookings.Any())
            {
                var count = upcomingBookings.Count();
                return Error.Conflict("barber.has.upcoming.bookings",
                    $"Cannot remove barber with {count} upcoming bookings. Please cancel or complete them first.");
            }

            // Remove Barber role
            await _userManager.RemoveFromRoleAsync(barber, "Barber");

            // Deactivate the barber (not deleting, to preserve historical booking data)
            barber.IsActive = false;
            barber.AcceptingBookings = false;
            barber.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(barber);

            return Result.Success;
        }
    }
}