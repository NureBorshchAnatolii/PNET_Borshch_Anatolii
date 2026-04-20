using Application.Models;

namespace Application.Contacts;

public interface IReportService
{
    Task<PassRateResponse> GetUserPassRateAsync(Guid userId);
    Task<IEnumerable<TestReportResponse>> GetUnderperformingTestsAsync();
    Task<IEnumerable<LeaderboardEntry>> GetTestLeaderboardAsync(Guid testId);
}