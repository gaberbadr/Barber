using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetTopBarbers
{
    public class GetTopBarbersQueryHandler : IRequestHandler<GetTopBarbersQuery, ErrorOr<List<TopBarberDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetTopBarbersQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ErrorOr<List<TopBarberDTO>>> Handle(GetTopBarbersQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var validBookingStatuses = new[] { BookingStatus.Confirmed, BookingStatus.Arrived, BookingStatus.DidNotArrive };
            var confirmedBookings = await bookingRepo.FindAsync(b => validBookingStatuses.Contains(b.Status));

            var topBarbers = confirmedBookings
                .GroupBy(b => b.BarberId)
                .Select(g => new
                {
                    BarberId = g.Key,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(b => b.BookingCount)
                .Take(request.Count)
                .ToList();

            var result = new List<TopBarberDTO>();
            foreach (var item in topBarbers)
            {
                var barber = await _userManager.FindByIdAsync(item.BarberId);
                result.Add(new TopBarberDTO
                {
                    BarberId = item.BarberId,
                    BarberName = barber?.FullName ?? "Unknown",
                    ConfirmedBookingCount = item.BookingCount,
                    TotalRevenue = item.TotalRevenue
                });
            }

            return result;
        }
    }
}