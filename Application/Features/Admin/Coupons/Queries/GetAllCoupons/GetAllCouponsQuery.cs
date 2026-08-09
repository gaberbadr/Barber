using MediatR;
using ErrorOr;
using Application.Features.Coupons.DTOs;

namespace Application.Features.Admin.Coupons.Queries.GetAllCoupons
{
    public class GetAllCouponsQuery : IRequest<ErrorOr<List<CouponDTO>>>
    {
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}