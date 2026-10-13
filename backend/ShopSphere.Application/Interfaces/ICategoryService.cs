using ShopSphere.Application.DTOs.Category;

namespace ShopSphere.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
        Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request);
        Task DeleteAsync(int id);
    }
}