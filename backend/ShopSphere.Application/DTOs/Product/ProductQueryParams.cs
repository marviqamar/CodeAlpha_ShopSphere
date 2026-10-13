namespace ShopSphere.Application.DTOs.Product
{
    public class ProductQueryParams
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string SortBy { get; set; } = "CreatedAt"; // Name, Price, CreatedAt
        public bool SortDescending { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}