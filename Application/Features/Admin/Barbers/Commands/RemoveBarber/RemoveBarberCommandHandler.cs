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
        private readonly TimeProvider _timeProvider;

        public RemoveBarberCommandHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<Success>> Handle(RemoveBarberCommand request, CancellationToken cancellationToken)
        {
            var barber = await _userManager.FindByIdAsync(request.BarberId);
            if (barber == null)
                return Error.NotFound("barber.not.found", "الحلاق ده مش موجود.");

            var isBarber = await _userManager.IsInRoleAsync(barber, "Barber");
            if (!isBarber)
                return Error.Forbidden("barber.not.barber", "المستخدم ده مش حلاق.");

            // Check for active/upcoming bookings
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var upcomingBookings = await bookingRepo.FindAsync(b =>
                b.BarberId == request.BarberId &&
                b.BookingDate >= DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date) &&
                b.Status == BookingStatus.Confirmed);

            if (upcomingBookings.Any())
            {
                var count = upcomingBookings.Count();
                return Error.Conflict("barber.has.upcoming.bookings",
                    $"مينفعش تحذف حلاق عنده {count} حجوزات جاية. يرجى إلغائها أو إتمامها الأول.");
            }

            // Remove Barber role
            await _userManager.RemoveFromRoleAsync(barber, "Barber");

            // Deactivate the barber (not deleting, to preserve historical booking data)
            barber.IsActive = false;
            barber.AcceptingBookings = false;
            barber.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

            await _userManager.UpdateAsync(barber);

            return Result.Success;
        }
    }
}