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
        private readonly TimeProvider _timeProvider;

        public GetDashboardStatsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<DashboardStatsDTO>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var barbers = await _userManager.GetUsersInRoleAsync("Barber");
            var barberIds = barbers.Select(b => b.Id).ToList();

            var totalUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted && !barberIds.Contains(u.Id), cancellationToken);
            var activeUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted && !barberIds.Contains(u.Id) && u.IsActive, cancellationToken);
            var blockedUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted && !barberIds.Contains(u.Id) && !u.IsActive, cancellationToken);

            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var bookingQuery = bookingRepo.GetIQueryable();

            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var totalServices = await serviceRepo.GetIQueryable().CountAsync(s => !s.IsDeleted, cancellationToken);

            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
            var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

            var validBookingStatuses = new[] { BookingStatus.Confirmed, BookingStatus.Arrived, BookingStatus.DidNotArrive };
            
            var totalConfirmedBookings = await bookingQuery.CountAsync(b => validBookingStatuses.Contains(b.Status), cancellationToken);
            var totalCancelledBookings = await bookingQuery.CountAsync(b => b.Status == BookingStatus.Cancelled, cancellationToken);
            
            var todayConfirmedBookings = await bookingQuery.CountAsync(b => validBookingStatuses.Contains(b.Status) && b.BookingDate == today, cancellationToken);
            var thisMonthConfirmedBookings = await bookingQuery.CountAsync(b => validBookingStatuses.Contains(b.Status) && b.BookingDate >= firstOfMonth, cancellationToken);
            
            var totalConfirmedRevenue = await bookingQuery.Where(b => validBookingStatuses.Contains(b.Status)).SumAsync(b => b.TotalPrice, cancellationToken);
            var thisMonthConfirmedRevenue = await bookingQuery.Where(b => validBookingStatuses.Contains(b.Status) && b.BookingDate >= firstOfMonth).SumAsync(b => b.TotalPrice, cancellationToken);

            var dto = new DashboardStatsDTO
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                BlockedUsers = blockedUsers,
                TotalBarbers = barbers.Count(b => !b.IsDeleted),
                ActiveBarbers = barbers.Count(b => !b.IsDeleted && b.IsActive),
                TotalServices = totalServices,
                TotalConfirmedBookings = totalConfirmedBookings,
                TotalCancelledBookings = totalCancelledBookings,
                TodayConfirmedBookings = todayConfirmedBookings,
                ThisMonthConfirmedBookings = thisMonthConfirmedBookings,
                TotalConfirmedRevenue = totalConfirmedRevenue,
                ThisMonthConfirmedRevenue = thisMonthConfirmedRevenue
            };

            return dto;
        }
    }
}