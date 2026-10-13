using ShopSphere.Application.DTOs;
using ShopSphere.Application.DTOs.Order;

namespace ShopSphere.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(int userId, PlaceOrderRequest request);
        Task<OrderDto> GetByIdAsync(int userId, int orderId);
        Task<List<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> CancelOrderAsync(int userId, int orderId);

        // Admin
        Task<PagedResult<OrderDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request);
    }
}