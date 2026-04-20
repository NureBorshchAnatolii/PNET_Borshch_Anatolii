using Application.Contacts;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _db;
    public TestRepository(ApplicationDbContext db) => _db = db;

    public async Task<Test?> GetByIdAsync(Guid id, bool includeQuestions = false)
    {
        var query = _db.Tests.AsQueryable();
        if (includeQuestions)
            query = query.Include(t => t.Questions).ThenInclude(q => q.Answers);
        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Test>> GetAllAsync() =>
        await _db.Tests
            .Include(t => t.Category)
            .Include(t => t.Questions)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Test>> GetByUserIdAsync(Guid userId) =>
        await _db.Tests.Where(t => t.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Test>> GetByCategoryIdAsync(Guid categoryId) =>
        await _db.Tests.Where(t => t.CategoryId == categoryId).ToListAsync();

    public async Task AddAsync(Test test)
    {
        await _db.Tests.AddAsync(test);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Test test)
    {
        _db.Tests.Update(test);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var test = await _db.Tests.FindAsync(id);
        if (test is not null)
        {
            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id) =>
        await _db.Tests.AnyAsync(t => t.Id == id);
}