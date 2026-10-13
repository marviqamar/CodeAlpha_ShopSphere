namespace ShopSphere.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key: which category this product belongs to
        public int CategoryId { get; set; }

        // Navigation property: the actual Category object
        public Category Category { get; set; } = null!;

        // Navigation property: a product can have many images
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        // Navigation property: a product can have many reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}