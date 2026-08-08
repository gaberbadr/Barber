using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, ErrorOr<DashboardStatsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetDashboardStatsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ErrorOr<DashboardStatsDTO>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var allUsers = await _userManager.Users.Where(u => !u.IsDeleted).ToListAsync(cancellationToken);
            var barbers = await _userManager.GetUsersInRoleAsync("Barber");
            var barberIds = barbers.Select(b => b.Id).ToHashSet();

            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var allBookings = await bookingRepo.GetAllAsync();

            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var services = await serviceRepo.FindAsync(s => !s.IsDeleted);

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

            var confirmedBookings = allBookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
            var cancelledBookings = allBookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();

            var dto = new DashboardStatsDTO
            {
                TotalUsers = allUsers.Count(u => !barberIds.Contains(u.Id)),
                ActiveUsers = allUsers.Count(u => !barberIds.Contains(u.Id) && u.IsActive),
                BlockedUsers = allUsers.Count(u => !barberIds.Contains(u.Id) && !u.IsActive),
                TotalBarbers = barbers.Count(b => !b.IsDeleted),
                ActiveBarbers = barbers.Count(b => !b.IsDeleted && b.IsActive),
                TotalServices = services.Count(),
                TotalConfirmedBookings = confirmedBookings.Count,
                TotalCancelledBookings = cancelledBookings.Count,
                TodayConfirmedBookings = confirmedBookings.Count(b => b.BookingDate == today),
                ThisMonthConfirmedBookings = confirmedBookings.Count(b => b.BookingDate >= firstOfMonth),
                TotalConfirmedRevenue = confirmedBookings.Sum(b => b.TotalPrice),
                ThisMonthConfirmedRevenue = confirmedBookings.Where(b => b.BookingDate >= firstOfMonth).Sum(b => b.TotalPrice)
            };

            return dto;
        }
    }
}