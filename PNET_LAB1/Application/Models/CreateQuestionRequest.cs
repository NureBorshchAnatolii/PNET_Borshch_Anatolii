using Domain.Enums;

namespace Application.Models;

public record CreateQuestionRequest(
    string Text,
    QuestionType QuestionType,
    decimal Points,
    int Order,
    string? Explanation,
    List<CreateAnswerRequest> Answers
);