using Application.Features.Coupons.DTOs;
using ErrorOr;
using MediatR;

namespace Application.Features.Coupons.Queries.GetByCode
{
    public class GetCouponByCodeQuery : IRequest<ErrorOr<CouponValueDTO>>
    {
        public string Code { get; set; } = string.Empty;
    }
}
