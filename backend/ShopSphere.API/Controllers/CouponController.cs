using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Coupon;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class CouponController : BaseApiController
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // Customer: check a code before checkout
        [HttpPost("validate")]
        public async Task<IActionResult> Validate(ValidateCouponRequest request)
        {
            var result = await _couponService.ValidateAsync(request.Code, request.OrderAmount);
            return Ok(result);
        }

        // Admin: list all coupons
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _couponService.GetAllAsync());
        }

        // Admin: create a coupon
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCouponRequest request)
        {
            var coupon = await _couponService.CreateAsync(request);
            return StatusCode(201, coupon);
        }

        // Admin: delete a coupon
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponService.DeleteAsync(id);
            return NoContent();
        }
    }
}