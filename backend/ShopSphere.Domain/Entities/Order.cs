using ShopSphere.Domain.Enums;

namespace ShopSphere.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Shipping snapshot (copied from Address at order time, not linked live)
        public string ShippingFullName { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }

        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}