using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs;
using ShopSphere.Application.DTOs.Product;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException($"Product with id {id} not found.");

            return MapToDto(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                throw new NotFoundException($"Category with id {request.CategoryId} not found.");

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Sku = request.Sku,
                CategoryId = request.CategoryId,
                Images = request.ImageUrls.Select((url, index) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0
                }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Reload with relations for a complete DTO
            return await GetByIdAsync(product.Id);
        }

        public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _context.Products.FindAsync(id)
                ?? throw new NotFoundException($"Product with id {id} not found.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                throw new NotFoundException($"Category with id {request.CategoryId} not found.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.Sku = request.Sku;
            product.CategoryId = request.CategoryId;
            product.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(product.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id)
                ?? throw new NotFoundException($"Product with id {id} not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParams queryParams)
        {
            // Start with a base query — nothing is executed against the DB yet
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Search filter (matches name OR description)
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search));
            }

            // Category filter
            if (queryParams.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);
            }

            // Price range filters
            if (queryParams.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= queryParams.MinPrice.Value);
            }

            if (queryParams.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);
            }

            // Sorting
            query = queryParams.SortBy.ToLower() switch
            {
                "name" => queryParams.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

                "price" => queryParams.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                _ => queryParams.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };

            // Total count BEFORE pagination (needed for TotalPages)
            var totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = products.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Sku = product.Sku,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
                AverageRating = product.Reviews.Any() ? Math.Round(product.Reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = product.Reviews.Count
            };
        }
    }
}