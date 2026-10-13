using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs;
using ShopSphere.Application.DTOs.Order;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ICouponService _couponService;

        public OrderService(AppDbContext context, ICouponService couponService)
        {
            _context = context;
            _couponService = couponService;
        }

        public async Task<OrderDto> PlaceOrderAsync(int userId, PlaceOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                    throw new ValidationException("Your cart is empty.");

                foreach (var item in cart.Items)
                {
                    if (item.Product.StockQuantity < item.Quantity)
                        throw new ValidationException(
                            $"Only {item.Product.StockQuantity} units of '{item.Product.Name}' are available.");
                }

                var order = new Order
                {
                    UserId = userId,
                    Status = OrderStatus.Pending,
                    ShippingFullName = request.ShippingFullName,
                    ShippingPhone = request.ShippingPhone,
                    ShippingStreet = request.ShippingStreet,
                    ShippingCity = request.ShippingCity,
                    ShippingState = request.ShippingState,
                    ShippingPostalCode = request.ShippingPostalCode,
                    ShippingCountry = request.ShippingCountry,
                    Items = cart.Items.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.Price
                    }).ToList()
                };

                var subtotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
                order.DiscountAmount = 0;

                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                {
                    var result = await _couponService.ValidateAsync(request.CouponCode, subtotal);

                    order.CouponCode = result.Code;
                    order.DiscountAmount = result.DiscountAmount;

                    var coupon = await _context.Coupons.FindAsync(result.CouponId);
                    coupon!.UsedCount++;
                }

                order.TotalAmount = subtotal - order.DiscountAmount;

                _context.Orders.Add(order);

                foreach (var item in cart.Items)
                {
                    item.Product.StockQuantity -= item.Quantity;
                }

                _context.CartItems.RemoveRange(cart.Items);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(userId, order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> GetByIdAsync(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            return MapToDto(order);
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<PagedResult<OrderDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            order.Status = request.Status;
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        public async Task<OrderDto> CancelOrderAsync(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            if (order.Status != OrderStatus.Pending)
                throw new ValidationException("Only pending orders can be cancelled.");

            foreach (var item in order.Items)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CouponCode = order.CouponCode,
                DiscountAmount = order.DiscountAmount,
                CustomerName = order.User.FullName,
                CustomerEmail = order.User.Email ?? string.Empty,
                ShippingFullName = order.ShippingFullName,
                ShippingPhone = order.ShippingPhone,
                ShippingStreet = order.ShippingStreet,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                               ?? i.Product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}