using ShopSphere.Application.DTOs.Cart;

namespace ShopSphere.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request);
        Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
        Task<CartDto> RemoveItemAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
    }
}