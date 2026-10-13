using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Cart;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize] // every endpoint in this controller requires a logged-in user
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return Ok(cart);
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem(AddToCartRequest request)
        {
            var cart = await _cartService.AddToCartAsync(GetUserId(), request);
            return Ok(cart);
        }

        // PUT: api/cart/items/5
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartItemRequest request)
        {
            var cart = await _cartService.UpdateItemAsync(GetUserId(), cartItemId, request);
            return Ok(cart);
        }

        // DELETE: api/cart/items/5
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var cart = await _cartService.RemoveItemAsync(GetUserId(), cartItemId);
            return Ok(cart);
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return NoContent();
        }
    }
}