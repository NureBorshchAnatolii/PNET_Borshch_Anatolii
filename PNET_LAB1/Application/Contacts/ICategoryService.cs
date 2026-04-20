using Application.Models;

namespace Application.Contacts;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task DeleteAsync(Guid id);
}