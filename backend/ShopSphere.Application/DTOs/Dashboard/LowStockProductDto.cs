namespace ShopSphere.Application.DTOs.Dashboard
{
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}