using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Wishlist;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : BaseApiController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            var items = await _wishlistService.GetMyWishlistAsync(GetUserId());
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddToWishlistRequest request)
        {
            var item = await _wishlistService.AddAsync(GetUserId(), request);
            return StatusCode(201, item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            await _wishlistService.RemoveAsync(GetUserId(), id);
            return NoContent();
        }
    }
}