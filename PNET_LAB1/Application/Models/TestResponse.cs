namespace Application.Models;

public record TestResponse(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    decimal PassingScore,
    int QuestionCount,
    DateTime CreatedAt
);