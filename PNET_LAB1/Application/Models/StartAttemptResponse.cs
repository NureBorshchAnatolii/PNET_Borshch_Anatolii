namespace Application.Models;

public record StartAttemptResponse(
    Guid AttemptId,
    Guid TestId,
    string TestTitle,
    int? TimeLimitMinutes,
    DateTime StartedAt,
    List<QuestionResponse> Questions
);