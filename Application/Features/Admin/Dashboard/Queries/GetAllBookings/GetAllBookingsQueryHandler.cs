using Application.Common.Pagination;
using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetAllBookings
{
    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, ErrorOr<PaginationResponse<AdminBookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAllBookingsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ErrorOr<PaginationResponse<AdminBookingDTO>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var bookings = await bookingRepo.FindAsync(b =>
                (!request.Date.HasValue || b.BookingDate == request.Date.Value) &&
                (string.IsNullOrEmpty(request.BarberId) || b.BarberId == request.BarberId) &&
                (string.IsNullOrEmpty(request.CustomerId) || b.CustomerId == request.CustomerId) &&
                (string.IsNullOrEmpty(request.Status) || b.Status.ToString() == request.Status));

            var totalCount = bookings.Count();

            var bookingList = bookings
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var result = new List<AdminBookingDTO>();
            foreach (var booking in bookingList)
            {
                var barber = await _userManager.FindByIdAsync(booking.BarberId);

                result.Add(new AdminBookingDTO
                {
                    Id = booking.Id,
                    CustomerName = booking.CustomerNameSnapshot ?? "Unknown",
                    CustomerPhone = booking.CustomerPhoneSnapshot,
                    CustomerEmail = booking.Customer?.Email ?? "",
                    BarberName = barber?.FullName ?? "Unknown",
                    BookingDate = booking.BookingDate,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status.ToString(),
                    CreatedAt = booking.CreatedAt
                });
            }

            return new PaginationResponse<AdminBookingDTO>(request.PageSize, request.PageIndex, totalCount, result);
        }
    }
}