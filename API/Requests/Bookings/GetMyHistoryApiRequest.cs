using Microsoft.AspNetCore.Mvc;

namespace Requests.Bookings
{
    public class GetMyHistoryApiRequest
    {
        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;
    }
}
