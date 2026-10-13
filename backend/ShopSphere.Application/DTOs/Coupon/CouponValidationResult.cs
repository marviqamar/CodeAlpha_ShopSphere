namespace ShopSphere.Application.DTOs.Coupon
{
    public class CouponValidationResult
    {
        public int CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal NewTotal { get; set; }
    }
}