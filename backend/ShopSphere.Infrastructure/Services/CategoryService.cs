using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Category;
using ShopSphere.Application.Exceptions;
using ShopSphere.Application.Interfaces;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id)
                ?? throw new NotFoundException($"Category with id {id} not found.");

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id)
                ?? throw new NotFoundException($"Category with id {id} not found.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id)
                ?? throw new NotFoundException($"Category with id {id} not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}