namespace Application.Models;

public record LeaderboardEntry(
    long Rank,
    Guid UserId,
    string FullName,
    string Email,
    decimal Score,
    int AttemptNumber,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int Passed
);