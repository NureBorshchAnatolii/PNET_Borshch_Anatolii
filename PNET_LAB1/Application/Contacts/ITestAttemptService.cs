using Application.Models;
using Domain.Model;

namespace Application.Contacts;

public interface ITestAttemptService
{
    Task<StartAttemptResponse> StartAttemptAsync(Guid userId, Guid testId);
    Task<AttemptResultResponse> SubmitAttemptAsync(Guid userId, Guid attemptId, SubmitAttemptRequest request);
    Task<AttemptResultResponse?> GetAttemptResultAsync(Guid userId, Guid attemptId);
    Task<IEnumerable<AttemptSummaryResponse>> GetUserAttemptsAsync(Guid userId);
    Task<IEnumerable<AttemptResultResponse>> GetAttemptsByTestIdAsync(Guid testId);
}