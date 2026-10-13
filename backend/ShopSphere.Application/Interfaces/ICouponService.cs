using ShopSphere.Application.DTOs.Coupon;

namespace ShopSphere.Application.Interfaces
{
    public interface ICouponService
    {
        Task<List<CouponDto>> GetAllAsync();
        Task<CouponDto> CreateAsync(CreateCouponRequest request);
        Task DeleteAsync(int id);
        Task<CouponValidationResult> ValidateAsync(string code, decimal orderAmount);
    }
}