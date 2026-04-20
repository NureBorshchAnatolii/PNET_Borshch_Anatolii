namespace Application.Models;

public record CreateTestRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ShuffleQuestions,
    bool ShowCorrectAnswers,
    decimal PassingScore,
    List<CreateQuestionRequest> Questions
);