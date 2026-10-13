using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Dashboard;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Enums;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        // Only these statuses count as real sales
        private static readonly OrderStatus[] RevenueStatuses =
            { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered };

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var stats = new DashboardStatsDto();

            // 1. Simple totals
            stats.TotalRevenue = await _context.Orders
                .Where(o => RevenueStatuses.Contains(o.Status))
                .SumAsync(o => o.TotalAmount);

            stats.TotalOrders = await _context.Orders.CountAsync();
            stats.TotalProducts = await _context.Products.CountAsync(p => p.IsActive);

            // Customers = users who have the "Customer" role
            var customerRoleId = await _context.Roles
                .Where(r => r.Name == "Customer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            stats.TotalCustomers = await _context.UserRoles
                .CountAsync(ur => ur.RoleId == customerRoleId);

            // 2. Orders grouped by status
            var statusCounts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Include every status, even those with 0 orders, so the frontend always gets all of them
            foreach (var status in Enum.GetValues<OrderStatus>())
            {
                stats.OrdersByStatus[status.ToString()] =
                    statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;
            }

            // 3. Top 5 selling products
            stats.TopProducts = await _context.OrderItems
                .Where(oi => RevenueStatuses.Contains(oi.Order.Status))
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToListAsync();

            // 4. Sales for each of the last 7 days
            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            var recentOrders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && RevenueStatuses.Contains(o.Status))
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var day = startDate.AddDays(i);
                var ordersOfDay = recentOrders.Where(o => o.OrderDate.Date == day).ToList();

                stats.SalesLast7Days.Add(new DailySalesDto
                {
                    Date = day,
                    OrderCount = ordersOfDay.Count,
                    Revenue = ordersOfDay.Sum(o => o.TotalAmount)
                });
            }

            // 5. Low stock products
            stats.LowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity <= 5)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            return stats;
        }
    }
}