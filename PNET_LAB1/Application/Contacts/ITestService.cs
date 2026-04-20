using Application.Models;

namespace Application.Contacts;

public interface ITestService
{
    Task<TestResponse> CreateTestAsync(Guid userId, CreateTestRequest request);
    Task<TestDetailResponse?> GetTestByIdAsync(Guid id, bool isOwnerOrAdmin = false);
    Task<IEnumerable<TestResponse>> GetAllTestsAsync();
    Task<IEnumerable<TestResponse>> GetTestsByUserAsync(Guid userId);
    Task UpdateTestAsync(Guid testId, Guid requestingUserId, CreateTestRequest request);
    Task DeleteTestAsync(Guid testId, Guid requestingUserId);
    Task<IEnumerable<TestResponse>> GetByCategoryIdAsync(Guid categoryId);
}