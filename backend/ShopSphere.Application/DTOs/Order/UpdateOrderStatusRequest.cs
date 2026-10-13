using ShopSphere.Domain.Enums;

namespace ShopSphere.Application.DTOs.Order
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}