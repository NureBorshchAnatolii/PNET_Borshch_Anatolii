namespace Application.Models;

public record TestDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ShuffleQuestions,
    bool ShowCorrectAnswers,
    decimal PassingScore,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    List<QuestionResponse> Questions
);