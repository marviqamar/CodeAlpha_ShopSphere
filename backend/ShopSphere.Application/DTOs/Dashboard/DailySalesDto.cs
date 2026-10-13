namespace ShopSphere.Application.DTOs.Dashboard
{
    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}