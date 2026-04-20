namespace Application.Models;

public record AnswerResponse(Guid Id, string Text, int Order, bool? IsCorrect);