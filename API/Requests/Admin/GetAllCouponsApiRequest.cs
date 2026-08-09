using Microsoft.AspNetCore.Mvc;

namespace Requests.Admin
{
    public class GetAllCouponsApiRequest
    {
        [FromQuery(Name = "isActive")]
        public bool? IsActive { get; set; }

        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 20;
    }
}
