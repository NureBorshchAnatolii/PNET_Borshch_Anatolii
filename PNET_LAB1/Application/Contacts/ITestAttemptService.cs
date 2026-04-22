using Application.Models;

namespace Application.Contacts;

public interface ITestAttemptService
{
    Task<StartAttemptResponse> StartAttemptAsync(Guid userId, Guid testId);
    Task SubmitAttemptAsync(Guid userId, Guid attemptId, SubmitAttemptRequest request);
    Task<AttemptResultResponse?> GetAttemptResultAsync(Guid userId, Guid attemptId);
    Task<IEnumerable<AttemptSummaryResponse>> GetUserAttemptsAsync(Guid userId);
    Task<IEnumerable<AttemptResultResponse>> GetAttemptsByTestIdAsync(Guid testId);
    Task<StartAttemptResponse?> GetAttemptDetailAsync(Guid userId, Guid attemptId);
}