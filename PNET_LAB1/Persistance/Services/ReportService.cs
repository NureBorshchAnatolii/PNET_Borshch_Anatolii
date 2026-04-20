using Application.Contacts;
using Application.Models;
using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;

    public ReportService(ApplicationDbContext db) => _db = db;

    public async Task<PassRateResponse> GetUserPassRateAsync(Guid userId)
    {
        var exists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
            throw new KeyNotFoundException("User not found.");

        var rate = await _db.Database
            .SqlQueryRaw<decimal>(
                "SELECT dbo.GetUserPassRate({0}) AS Value", userId)
            .FirstOrDefaultAsync();

        return new PassRateResponse(userId, rate);
    }

    public async Task<IEnumerable<TestReportResponse>> GetUnderperformingTestsAsync()
    {
        return await _db.Database
            .SqlQueryRaw<TestReportResponse>("EXEC FlagUnderperformingTests")
            .ToListAsync();
    }
    
    public async Task<IEnumerable<LeaderboardEntry>> GetTestLeaderboardAsync(Guid testId)
    {
        var exists = await _db.Tests.AnyAsync(t => t.Id == testId);
        if (!exists)
            throw new KeyNotFoundException("Test not found.");

        return await _db.Database
            .SqlQueryRaw<LeaderboardEntry>(
                "SELECT * FROM dbo.GetTestLeaderboard({0})", testId)
            .ToListAsync();
    }
}