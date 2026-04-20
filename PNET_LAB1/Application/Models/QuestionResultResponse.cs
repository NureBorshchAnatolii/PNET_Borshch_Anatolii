namespace Application.Models;

public record QuestionResultResponse(
    Guid QuestionId,
    string QuestionText,
    bool IsCorrect,
    decimal Points,
    decimal PointsEarned,
    string? Explanation
);