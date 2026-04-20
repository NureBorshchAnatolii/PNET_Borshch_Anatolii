using Domain.Model;

namespace Application.Contacts;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Answer>> GetByQuestionIdAsync(Guid questionId);
    Task AddRangeAsync(IEnumerable<Answer> answers);
    Task UpdateAsync(Answer answer);
    Task DeleteByQuestionIdAsync(Guid questionId);
}