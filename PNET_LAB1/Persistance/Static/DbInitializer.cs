using Microsoft.EntityFrameworkCore;
using Persistance.DbContext;

namespace Persistance.Static;

public static class DbInitializer
{
    public static async Task InitializeDbObjectsAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(Scripts.GetUserPassRate);
        await db.Database.ExecuteSqlRawAsync(Scripts.GetTestLeaderboard);
        await db.Database.ExecuteSqlRawAsync(Scripts.PreventTestDeleteWithAttempts);
        await db.Database.ExecuteSqlRawAsync(Scripts.FlagUnderperformingTests);
        await db.Database.ExecuteSqlRawAsync(Scripts.RecalculateScoreAfterAnswer);
    }
}