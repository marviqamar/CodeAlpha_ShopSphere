using ShopSphere.Application.DTOs.Review;

namespace ShopSphere.Application.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetByProductIdAsync(int productId);
        Task<ReviewDto> CreateAsync(int userId, CreateReviewRequest request);
        Task<ReviewDto> UpdateAsync(int userId, int reviewId, UpdateReviewRequest request);
        Task DeleteAsync(int userId, int reviewId);
    }
}