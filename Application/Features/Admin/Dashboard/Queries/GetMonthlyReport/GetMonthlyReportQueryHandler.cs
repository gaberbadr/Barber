using Application.Features.Admin.Dashboard.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Error = ErrorOr.Error;

namespace Application.Features.Admin.Dashboard.Queries.GetMonthlyReport
{
    public class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, ErrorOr<List<MonthlyReportDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public GetMonthlyReportQueryHandler(IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<ErrorOr<List<MonthlyReportDTO>>> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
            DateOnly fromDate, toDate;

            switch (request.Period.ToLower())
            {
                case "thismonth":
                    fromDate = new DateOnly(today.Year, today.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case "previousmonth":
                    var prevMonth = today.AddMonths(-1);
                    fromDate = new DateOnly(prevMonth.Year, prevMonth.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case "thisyear":
                    fromDate = new DateOnly(today.Year, 1, 1);
                    toDate = new DateOnly(today.Year, 12, 31);
                    break;
                case "custom":
                    fromDate = request.FromDate ?? new DateOnly(today.Year, 1, 1);
                    toDate = request.ToDate ?? today;
                    break;
                case "alltime":
                default:
                    fromDate = new DateOnly(2020, 1, 1);
                    toDate = today;
                    break;
            }

            var bookingRepo = _unitOfWork.Repository<Booking, int>();
            var bookings = await bookingRepo.FindAsync(b =>
                b.BookingDate >= fromDate && b.BookingDate <= toDate);

            var monthlyData = bookings
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                .Select(g => new MonthlyReportDTO
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    ConfirmedBookingCount = g.Count(b => b.Status == BookingStatus.Confirmed),
                    CancelledBookingCount = g.Count(b => b.Status == BookingStatus.Cancelled),
                    TotalRevenue = g.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalPrice)
                })
                .OrderBy(m => m.Month)
                .ToList();

            return monthlyData;
        }
    }
}