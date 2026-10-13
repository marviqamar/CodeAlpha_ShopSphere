using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Cart;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId)
                ?? throw new NotFoundException($"Product with id {request.ProductId} not found.");

            if (request.Quantity < 1)
                throw new ValidationException("Quantity must be at least 1.");

            if (product.StockQuantity < request.Quantity)
                throw new ValidationException($"Only {product.StockQuantity} units of '{product.Name}' are available.");

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // Product already in cart — increase quantity instead of duplicating
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CartId = cart.Id
                });
            }

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
        {
            if (request.Quantity < 1)
                throw new ValidationException("Quantity must be at least 1.");

            var cart = await GetOrCreateCartAsync(userId);

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new NotFoundException("Cart item not found.");

            var product = await _context.Products.FindAsync(item.ProductId)
                ?? throw new NotFoundException("Product no longer exists.");

            if (product.StockQuantity < request.Quantity)
                throw new ValidationException($"Only {product.StockQuantity} units of '{product.Name}' are available.");

            item.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<CartDto> RemoveItemAsync(int userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new NotFoundException("Cart item not found.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }

        // Helper: every user should always have exactly one cart.
        // If they don't have one yet (e.g., first time adding to cart), create it automatically.
        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private static CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                               ?? i.Product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity,
                    AvailableStock = i.Product.StockQuantity
                }).ToList()
            };
        }
    }
}