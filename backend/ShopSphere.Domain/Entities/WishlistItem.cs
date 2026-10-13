namespace ShopSphere.Domain.Entities
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}