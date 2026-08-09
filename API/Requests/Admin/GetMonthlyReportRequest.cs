using System;
using Microsoft.AspNetCore.Mvc;

namespace Requests.Admin
{
    public class GetMonthlyReportRequest
    {
        [FromQuery(Name = "period")]
        public string Period { get; set; } = "ThisMonth";

        [FromQuery(Name = "fromDate")]
        public DateOnly? FromDate { get; set; }

        [FromQuery(Name = "toDate")]
        public DateOnly? ToDate { get; set; }
    }
}
