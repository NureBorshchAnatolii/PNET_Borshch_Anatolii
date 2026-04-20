using Domain.Enums;

namespace Application.Models;

public record QuestionResponse(
    Guid Id,
    string Text,
    QuestionType QuestionType,
    decimal Points,
    int Order,
    string? Explanation,
    List<AnswerResponse> Answers
);