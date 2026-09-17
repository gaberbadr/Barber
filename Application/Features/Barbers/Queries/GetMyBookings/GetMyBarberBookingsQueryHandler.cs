using Application.Common.Pagination;
using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Barbers.Queries.GetMyBookings
{
    public class GetMyBarberBookingsQueryHandler : IRequestHandler<GetMyBarberBookingsQuery, ErrorOr<PaginationResponse<BookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetMyBarberBookingsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<PaginationResponse<BookingDTO>>> Handle(GetMyBarberBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var query = bookingRepo.GetIQueryable()
                .Where(b => b.BarberId == request.BarberId &&
                            (!request.FromDate.HasValue || b.BookingDate >= request.FromDate.Value) &&
                            (!request.ToDate.HasValue || b.BookingDate <= request.ToDate.Value));

            var totalCount = await query.CountAsync(cancellationToken);

            var bookingsQuery = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BookingDTO
                {
                    Id = b.Id,
                    CustomerId = b.CustomerId,
                    CustomerName = (b.Customer != null ? b.Customer.FullName : b.CustomerNameSnapshot) ?? "",
                    CustomerPhone = (b.Customer != null ? b.Customer.PhoneNumber : b.CustomerPhoneSnapshot) ?? "Unknown",
                    BarberId = b.BarberId,
                    BarberName = (b.Barber != null ? b.Barber.FullName : null) ?? "",
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    SubTotal = b.SubTotal,
                    Discount = b.Discount,
                    TotalPrice = b.TotalPrice,
                    CouponCode = b.CouponCodeSnapshot,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                    CancelledAt = b.CancelledAt,
                    CancelledBy = b.CancelledBy,
                    Items = b.BookingItems.Select(bi => new BookingItemDTO
                    {
                        Id = bi.Id,
                        ServiceId = bi.ServiceId,
                        ServiceName = bi.ServiceNameSnapshot,
                        UnitPrice = bi.UnitPrice,
                        Quantity = bi.Quantity,
                        TotalPrice = bi.TotalPrice
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResponse<BookingDTO>(request.PageSize, request.PageIndex, totalCount, bookingsQuery);
        }
    }
}
