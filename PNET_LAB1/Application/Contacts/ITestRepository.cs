using Domain.Model;

namespace Application.Contacts;

public interface ITestRepository
{
    Task<Test?> GetByIdAsync(Guid id, bool includeQuestions = false);
    Task<IEnumerable<Test>> GetAllAsync();
    Task<IEnumerable<Test>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Test>> GetByCategoryIdAsync(Guid categoryId);
    Task AddAsync(Test test);
    Task UpdateAsync(Test test);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}