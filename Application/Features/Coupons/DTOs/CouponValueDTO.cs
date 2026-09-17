namespace Application.Features.Coupons.DTOs
{
    public class CouponValueDTO
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
    }
}
