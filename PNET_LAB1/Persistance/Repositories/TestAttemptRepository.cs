using Application.Contacts;
using Domain.Enums;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class TestAttemptRepository : ITestAttemptRepository
{
    private readonly ApplicationDbContext _db;
    public TestAttemptRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<TestAttempt>> GetByTestIdAsync(Guid testId) =>
        await _db.TestAttempts
            .Where(a => a.TestId == testId)
            .Include(a => a.Test)
            .OrderByDescending(a => a.StartedAt)
            .AsNoTracking()
            .ToListAsync();
    
    public async Task<TestAttempt?> GetByIdAsync(Guid id, bool includeUserAnswers = false)
    {
        var query = _db.TestAttempts.AsQueryable();
        if (includeUserAnswers)
            query = query.Include(a => a.UserAnswers);
        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<TestAttempt>> GetByUserIdAsync(Guid userId) =>
        await _db.TestAttempts.Where(a => a.UserId == userId)
            .Include(a => a.Test).ToListAsync();

    public async Task<int> CountAttemptsAsync(Guid userId, Guid testId) =>
        await _db.TestAttempts.CountAsync(a =>
            a.UserId == userId && a.TestId == testId &&
            a.Status == AttemptStatus.Completed);

    public async Task<TestAttempt?> GetActiveAttemptAsync(Guid userId, Guid testId) =>
        await _db.TestAttempts.FirstOrDefaultAsync(a =>
            a.UserId == userId && a.TestId == testId &&
            a.Status == AttemptStatus.InProgress);

    public async Task AddAsync(TestAttempt attempt)
    {
        await _db.TestAttempts.AddAsync(attempt);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TestAttempt attempt)
    {
        _db.TestAttempts.Update(attempt);
        await _db.SaveChangesAsync();
    }
}