using Domain.Model;

namespace Application.Contacts;

public interface IUserAnswerRepository
{
    Task<IEnumerable<UserAnswer>> GetByAttemptIdAsync(Guid attemptId);
    Task AddRangeAsync(IEnumerable<UserAnswer> answers);
    Task<bool> ExistsForAttemptAsync(Guid attemptId, Guid questionId);
}