using ShopSphere.Application.DTOs.Wishlist;

namespace ShopSphere.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetMyWishlistAsync(int userId);
        Task<WishlistItemDto> AddAsync(int userId, AddToWishlistRequest request);
        Task RemoveAsync(int userId, int wishlistItemId);
    }
}