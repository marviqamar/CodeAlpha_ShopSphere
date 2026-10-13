using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Coupon;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class CouponService : ICouponService
    {
        private readonly AppDbContext _context;

        public CouponService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CouponDto>> GetAllAsync()
        {
            var coupons = await _context.Coupons.OrderByDescending(c => c.Id).ToListAsync();
            return coupons.Select(MapToDto).ToList();
        }

        public async Task<CouponDto> CreateAsync(CreateCouponRequest request)
        {
            var code = request.Code.Trim().ToUpper();

            if (string.IsNullOrEmpty(code))
                throw new ValidationException("Coupon code is required.");

            if (request.DiscountValue <= 0)
                throw new ValidationException("Discount value must be greater than 0.");

            if (request.DiscountType == DiscountType.Percentage && request.DiscountValue > 100)
                throw new ValidationException("Percentage discount cannot exceed 100.");

            if (await _context.Coupons.AnyAsync(c => c.Code == code))
                throw new ConflictException("A coupon with this code already exists.");

            var coupon = new Coupon
            {
                Code = code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinOrderAmount = request.MinOrderAmount,
                MaxUses = request.MaxUses,
                ExpiryDate = request.ExpiryDate
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return MapToDto(coupon);
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id)
                ?? throw new NotFoundException("Coupon not found.");

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task<CouponValidationResult> ValidateAsync(string code, decimal orderAmount)
        {
            var normalized = code.Trim().ToUpper();

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalized)
                ?? throw new NotFoundException("Invalid coupon code.");

            if (!coupon.IsActive)
                throw new ValidationException("This coupon is no longer active.");

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow)
                throw new ValidationException("This coupon has expired.");

            if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
                throw new ValidationException("This coupon has reached its usage limit.");

            if (orderAmount < coupon.MinOrderAmount)
                throw new ValidationException($"A minimum order of {coupon.MinOrderAmount} is required for this coupon.");

            var discount = coupon.DiscountType == DiscountType.Percentage
                ? Math.Round(orderAmount * coupon.DiscountValue / 100, 2)
                : coupon.DiscountValue;

            // A discount can never be bigger than the order itself
            discount = Math.Min(discount, orderAmount);

            return new CouponValidationResult
            {
                CouponId = coupon.Id,
                Code = coupon.Code,
                DiscountAmount = discount,
                NewTotal = orderAmount - discount
            };
        }

        private static CouponDto MapToDto(Coupon c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinOrderAmount = c.MinOrderAmount,
            MaxUses = c.MaxUses,
            UsedCount = c.UsedCount,
            ExpiryDate = c.ExpiryDate,
            IsActive = c.IsActive
        };
    }
}