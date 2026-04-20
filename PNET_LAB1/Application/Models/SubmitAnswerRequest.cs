namespace Application.Models;

public record SubmitAnswerRequest(
    Guid QuestionId,
    List<Guid>? SelectedAnswerIds,
    string? TextAnswer
);