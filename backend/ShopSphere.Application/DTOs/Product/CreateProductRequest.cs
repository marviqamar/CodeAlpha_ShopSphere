namespace ShopSphere.Application.DTOs.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}