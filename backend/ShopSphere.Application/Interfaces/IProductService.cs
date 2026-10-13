using ShopSphere.Application.DTOs;
using ShopSphere.Application.DTOs.Product;

namespace ShopSphere.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParams queryParams);
        Task<ProductDto> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductRequest request);
        Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);
        Task DeleteAsync(int id);
    }
}