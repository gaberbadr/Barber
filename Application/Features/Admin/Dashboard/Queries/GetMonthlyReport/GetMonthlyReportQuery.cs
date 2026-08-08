using MediatR;
using ErrorOr;
using Application.Features.Admin.Dashboard.DTOs;

namespace Application.Features.Admin.Dashboard.Queries.GetMonthlyReport
{
    public class GetMonthlyReportQuery : IRequest<ErrorOr<List<MonthlyReportDTO>>>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string Period { get; set; } = "ThisMonth"; // ThisMonth, PreviousMonth, ThisYear, AllTime, Custom
    }
}