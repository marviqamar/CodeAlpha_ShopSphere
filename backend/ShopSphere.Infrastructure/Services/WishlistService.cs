using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Wishlist;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WishlistItemDto>> GetMyWishlistAsync(int userId)
        {
            var items = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            return items.Select(MapToDto).ToList();
        }

        public async Task<WishlistItemDto> AddAsync(int userId, AddToWishlistRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId)
                ?? throw new NotFoundException("Product not found.");

            var alreadyExists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == request.ProductId);

            if (alreadyExists)
                throw new ConflictException("This product is already in your wishlist.");

            var item = new WishlistItem
            {
                UserId = userId,
                ProductId = request.ProductId
            };

            _context.WishlistItems.Add(item);
            await _context.SaveChangesAsync();

            var saved = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .FirstAsync(w => w.Id == item.Id);

            return MapToDto(saved);
        }

        public async Task RemoveAsync(int userId, int wishlistItemId)
        {
            var item = await _context.WishlistItems.FindAsync(wishlistItemId)
                ?? throw new NotFoundException("Wishlist item not found.");

            if (item.UserId != userId)
                throw new UnauthorizedException("You can only remove items from your own wishlist.");

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        private static WishlistItemDto MapToDto(WishlistItem item)
        {
            return new WishlistItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ImageUrl = item.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? item.Product.Images.FirstOrDefault()?.ImageUrl,
                Price = item.Product.Price,
                InStock = item.Product.StockQuantity > 0,
                AddedAt = item.AddedAt
            };
        }
    }
}