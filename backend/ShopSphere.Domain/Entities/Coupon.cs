using ShopSphere.Domain.Enums;

namespace ShopSphere.Domain.Entities
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }   // 0 = no minimum
        public int? MaxUses { get; set; }             // null = unlimited
        public int UsedCount { get; set; }
        public DateTime? ExpiryDate { get; set; }     // null = never expires
        public bool IsActive { get; set; } = true;
    }
}