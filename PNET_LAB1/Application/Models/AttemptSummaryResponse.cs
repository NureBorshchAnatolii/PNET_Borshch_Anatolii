namespace Application.Models;

public record AttemptSummaryResponse(
    Guid Id,
    Guid TestId,
    string TestTitle,
    decimal PassingScore,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string Status,
    decimal? Score,
    int AttemptNumber
);