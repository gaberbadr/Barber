using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetTopServices
{
    public class GetTopServicesQueryHandler : IRequestHandler<GetTopServicesQuery, ErrorOr<List<TopServiceDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTopServicesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<List<TopServiceDTO>>> Handle(GetTopServicesQuery request, CancellationToken cancellationToken)
        {
            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var validBookingStatuses = new[] { BookingStatus.Confirmed, BookingStatus.Arrived, BookingStatus.DidNotArrive };
            
            var confirmedBookingQuery = bookingRepo.GetIQueryable()
                .Where(b => validBookingStatuses.Contains(b.Status))
                .Select(b => b.Id);

            var bookingItemRepo = _unitOfWork.Repository<BookingItem, int>();
            
            var topServices = await bookingItemRepo.GetIQueryable()
                .Where(bi => confirmedBookingQuery.Contains(bi.BookingId))
                .GroupBy(bi => new { bi.ServiceId, bi.ServiceNameSnapshot })
                .Select(g => new TopServiceDTO
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceNameSnapshot,
                    BookingCount = g.Sum(bi => bi.Quantity),
                    TotalRevenue = g.Sum(bi => bi.TotalPrice)
                })
                .OrderByDescending(s => s.BookingCount)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            return topServices;
        }
    }
}