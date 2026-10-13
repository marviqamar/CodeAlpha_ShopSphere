using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Review;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewDto>> GetByProductIdAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToDto).ToList();
        }

        public async Task<ReviewDto> CreateAsync(int userId, CreateReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ValidationException("Rating must be between 1 and 5.");

            // Check the user already reviewed this product
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == request.ProductId && r.UserId == userId);

            if (alreadyReviewed)
                throw new ConflictException("You have already reviewed this product.");

            // Check the user has a delivered order containing this product
            var hasPurchased = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items)
                .AnyAsync(oi => oi.ProductId == request.ProductId);

            if (!hasPurchased)
                throw new ValidationException("You can only review products you have purchased and received.");

            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Reload with User included, for the DTO
            var saved = await _context.Reviews
                .Include(r => r.User)
                .FirstAsync(r => r.Id == review.Id);

            return MapToDto(saved);
        }

        public async Task<ReviewDto> UpdateAsync(int userId, int reviewId, UpdateReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ValidationException("Rating must be between 1 and 5.");

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new NotFoundException("Review not found.");

            if (review.UserId != userId)
                throw new UnauthorizedException("You can only edit your own reviews.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _context.SaveChangesAsync();

            return MapToDto(review);
        }

        public async Task DeleteAsync(int userId, int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId)
                ?? throw new NotFoundException("Review not found.");

            if (review.UserId != userId)
                throw new UnauthorizedException("You can only delete your own reviews.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        private static ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = review.User.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}