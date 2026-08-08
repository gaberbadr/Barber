namespace Application.Features.Coupons.DTOs
{
    public class CouponDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int TimesUsed { get; set; }
    }
}