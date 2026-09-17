using Application.Common.Pagination;
using Application.Features.Bookings.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zero.Core.Specification;
using Error = ErrorOr.Error;

namespace Application.Features.Bookings.Queries.GetMyHistory
{
    public class GetMyBookingHistoryQueryHandler : IRequestHandler<GetMyBookingHistoryQuery, ErrorOr<PaginationResponse<BookingDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetMyBookingHistoryQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ErrorOr<PaginationResponse<BookingDTO>>> Handle(GetMyBookingHistoryQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();

            var query = bookingRepo.GetIQueryable()
                .Where(b => b.CustomerId == request.CustomerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var bookingsQuery = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new
                {
                    Booking = b,
                    CustomerName = b.Customer != null ? b.Customer.FullName : null,
                    CustomerPhone = b.Customer != null ? b.Customer.PhoneNumber : null,
                    BarberName = b.Barber != null ? b.Barber.FullName : null,
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

            var dtos = new List<BookingDTO>();
            foreach (var item in bookingsQuery)
            {
                var dto = _mapper.Map<BookingDTO>(item.Booking);
                dto.CustomerName = item.CustomerName ?? "";
                dto.CustomerPhone = item.CustomerPhone;
                dto.BarberName = item.BarberName ?? "";
                dto.Items = item.Items;
                dtos.Add(dto);
            }

            return new PaginationResponse<BookingDTO>(request.PageSize, request.PageIndex, totalCount, dtos);
        }
    }
}