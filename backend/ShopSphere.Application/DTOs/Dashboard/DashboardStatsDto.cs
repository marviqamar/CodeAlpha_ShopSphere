namespace ShopSphere.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<DailySalesDto> SalesLast7Days { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }
}