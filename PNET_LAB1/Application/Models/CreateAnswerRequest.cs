namespace Application.Models;

public record CreateAnswerRequest(
    string Text,
    bool IsCorrect,
    int Order
);