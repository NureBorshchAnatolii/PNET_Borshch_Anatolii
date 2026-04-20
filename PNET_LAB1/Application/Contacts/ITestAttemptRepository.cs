using Domain.Model;

namespace Application.Contacts;

public interface ITestAttemptRepository
{
    Task<TestAttempt?> GetByIdAsync(Guid id, bool includeUserAnswers = false);
    Task<IEnumerable<TestAttempt>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<TestAttempt>> GetByTestIdAsync(Guid testId);
    Task<int> CountAttemptsAsync(Guid userId, Guid testId);
    Task<TestAttempt?> GetActiveAttemptAsync(Guid userId, Guid testId);
    Task AddAsync(TestAttempt attempt);
    Task UpdateAsync(TestAttempt attempt);
}