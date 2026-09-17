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

        public async Task<ErrorOr<PaginationResponse<AdminBookingDTO>>> Handle(
     GetAllBookingsQuery request,
     CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            BookingStatus? status = null;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<BookingStatus>(
                    request.Status,
                    ignoreCase: true,
                    out var parsedStatus))
                {
                    return Error.Validation(
                        code: "Booking.InvalidStatus",
                        description: $"حالة الحجز غير صحيحة: {request.Status}");
                }

                status = parsedStatus;
            }

            var query = bookingRepo.GetIQueryable()
                .AsNoTracking()
                .Where(b =>
                    (!request.Date.HasValue || b.BookingDate == request.Date.Value) &&
                    (string.IsNullOrEmpty(request.BarberId) || b.BarberId == request.BarberId) &&
                    (string.IsNullOrEmpty(request.CustomerId) || b.CustomerId == request.CustomerId) &&
                    (!status.HasValue || b.Status == status.Value));

            var totalCount = await query.CountAsync(cancellationToken);

            var result = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new AdminBookingDTO
                {
                    Id = b.Id,
                    CustomerName = b.CustomerNameSnapshot ?? "Unknown",
                    CustomerPhone = b.CustomerPhoneSnapshot,
                    CustomerEmail = b.Customer.Email ?? "",
                    BarberName = b.Barber.FullName ?? "Unknown",
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PaginationResponse<AdminBookingDTO>(
                request.PageSize,
                request.PageIndex,
                totalCount,
                result);
        }
    }
}
