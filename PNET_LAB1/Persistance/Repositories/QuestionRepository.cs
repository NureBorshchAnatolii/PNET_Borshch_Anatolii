using Application.Contacts;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _db;

    public QuestionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Question?> GetByIdAsync(Guid id, bool includeAnswers = false)
    {
        var query = _db.Questions.AsQueryable();

        if (includeAnswers)
            query = query.Include(q => q.Answers);

        return await query.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId)
    {
        return await _db.Questions
            .Where(q => q.TestId == testId)
            .Include(q => q.Answers)
            .OrderBy(q => q.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Question question)
    {
        await _db.Questions.AddAsync(question);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Question> questions)
    {
        await _db.Questions.AddRangeAsync(questions);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Question question)
    {
        _db.Questions.Update(question);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question is not null)
        {
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
        }
    }
}