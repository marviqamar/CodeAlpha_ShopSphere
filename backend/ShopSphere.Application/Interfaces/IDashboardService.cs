using ShopSphere.Application.DTOs.Dashboard;

namespace ShopSphere.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }
}