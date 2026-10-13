using ShopSphere.Domain.Enums;

namespace ShopSphere.Application.DTOs.Coupon
{
    public class CreateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int? MaxUses { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}