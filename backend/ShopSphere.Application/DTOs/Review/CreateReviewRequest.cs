namespace ShopSphere.Application.DTOs.Review
{
    public class CreateReviewRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}