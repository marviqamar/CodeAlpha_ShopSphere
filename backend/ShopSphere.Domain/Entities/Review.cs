namespace ShopSphere.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}