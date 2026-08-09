using MediatR;
using ErrorOr;
using Application.Features.Coupons.DTOs;

namespace Application.Features.Admin.Coupons.Commands.CreateCoupon
{
    public class CreateCouponCommand : IRequest<ErrorOr<CouponDTO>>
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
    }
}