using Application.Contacts;
using Application.Models;
using Domain.Model;

namespace Application.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.Tests.Count));
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name cannot be empty.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        await _categoryRepo.AddAsync(category);

        return new CategoryResponse(category.Id, category.Name, 0);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException("Category not found.");

        if (category.Tests.Count > 0)
            throw new InvalidOperationException(
                "Cannot delete a category that has tests assigned to it.");

        await _categoryRepo.DeleteAsync(id);
    }
}