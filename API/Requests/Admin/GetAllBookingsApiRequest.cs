using System;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;

namespace Requests.Admin
{
    public class GetAllBookingsApiRequest
    {
        [FromQuery(Name = "date")]
        public DateOnly? Date { get; set; }

        [FromQuery(Name = "barberId")]
        public string? BarberId { get; set; }

        [FromQuery(Name = "customerId")]
        public string? CustomerId { get; set; }

        [FromQuery(Name = "status")]
        public string? Status { get; set; }

        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 20;
    }
}
