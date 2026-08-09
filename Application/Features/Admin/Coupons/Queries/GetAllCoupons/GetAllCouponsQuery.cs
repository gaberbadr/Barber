using MediatR;
using ErrorOr;
using Application.Features.Coupons.DTOs;
using Application.Common.Pagination;

namespace Application.Features.Admin.Coupons.Queries.GetAllCoupons
{
    public class GetAllCouponsQuery : PaginationRequest, IRequest<ErrorOr<PaginationResponse<CouponDTO>>>
    {
        public bool? IsActive { get; set; }
    }
}