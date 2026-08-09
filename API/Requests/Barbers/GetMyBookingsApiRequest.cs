using System;
using Microsoft.AspNetCore.Mvc;

namespace Requests.Barbers
{
    public class GetMyBookingsApiRequest
    {
        [FromQuery(Name = "fromDate")]
        public DateOnly? FromDate { get; set; }

        [FromQuery(Name = "toDate")]
        public DateOnly? ToDate { get; set; }

        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;
    }
}
