using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Review;
using ShopSphere.Application.Interfaces;

namespace ShopSphere.API.Controllers
{
    [Route("api/[controller]")]
    public class ReviewController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/review/product/5   (public — anyone can read reviews)
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        // POST: api/review   (must be logged in)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateReviewRequest request)
        {
            var review = await _reviewService.CreateAsync(GetUserId(), request);
            return StatusCode(201, review);
        }

        // PUT: api/review/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateReviewRequest request)
        {
            var review = await _reviewService.UpdateAsync(GetUserId(), id, request);
            return Ok(review);
        }

        // DELETE: api/review/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _reviewService.DeleteAsync(GetUserId(), id);
            return NoContent();
        }
    }
}