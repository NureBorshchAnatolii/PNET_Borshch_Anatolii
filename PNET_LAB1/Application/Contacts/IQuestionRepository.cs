using Domain.Model;

namespace Application.Contacts;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid id, bool includeAnswers = false);
    Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId);
    Task AddAsync(Question question);
    Task AddRangeAsync(IEnumerable<Question> questions);
    Task UpdateAsync(Question question);
    Task DeleteAsync(Guid id);
}