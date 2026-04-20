using Application.Contacts;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class AnswerRepository : IAnswerRepository
{
    private readonly ApplicationDbContext _db;
    public AnswerRepository(ApplicationDbContext db) => _db = db;

    public async Task<Answer?> GetByIdAsync(Guid id) =>
        await _db.Answers.FindAsync(id);

    public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(Guid questionId) =>
        await _db.Answers
            .Where(a => a.QuestionId == questionId)
            .OrderBy(a => a.Order)
            .AsNoTracking()
            .ToListAsync();

    public async Task AddRangeAsync(IEnumerable<Answer> answers)
    {
        await _db.Answers.AddRangeAsync(answers);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Answer answer)
    {
        _db.Answers.Update(answer);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByQuestionIdAsync(Guid questionId)
    {
        var answers = await _db.Answers
            .Where(a => a.QuestionId == questionId)
            .ToListAsync();

        _db.Answers.RemoveRange(answers);
        await _db.SaveChangesAsync();
    }
}