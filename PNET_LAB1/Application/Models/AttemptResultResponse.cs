using Domain.Enums;

namespace Application.Models;

public record AttemptResultResponse(
    Guid AttemptId,
    decimal Score,
    decimal PassingScore,
    bool Passed,
    int TotalQuestions,
    int CorrectAnswers,
    DateTime StartedAt,
    DateTime FinishedAt,
    AttemptStatus Status,
    List<QuestionResultResponse>? QuestionResults
);