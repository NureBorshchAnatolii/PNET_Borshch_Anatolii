using Application.Contacts;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _db.Categories
            .Include(c => c.Tests)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(Guid id) =>
        await _db.Categories.FindAsync(id);

    public async Task AddAsync(Category category)
    {
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.Tests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}