using MediatR;
using ErrorOr;

namespace Application.Features.Admin.Coupons.Commands.DeleteCoupon
{
    public class DeleteCouponCommand : IRequest<ErrorOr<Success>>
    {
        public int CouponId { get; set; }
    }
}