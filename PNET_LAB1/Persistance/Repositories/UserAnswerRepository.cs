using Application.Contacts;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class UserAnswerRepository : IUserAnswerRepository
{
    private readonly ApplicationDbContext _db;

    public UserAnswerRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UserAnswer>> GetByAttemptIdAsync(Guid attemptId)
    {
        return await _db.UserAnswers
            .Where(ua => ua.AttemptId == attemptId)
            .Include(ua => ua.Question)
            .Include(ua => ua.SelectedAnswer)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<UserAnswer> answers)
    {
        await _db.UserAnswers.AddRangeAsync(answers);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsForAttemptAsync(Guid attemptId, Guid questionId)
    {
        return await _db.UserAnswers
            .AnyAsync(ua => ua.AttemptId == attemptId && ua.QuestionId == questionId);
    }
}