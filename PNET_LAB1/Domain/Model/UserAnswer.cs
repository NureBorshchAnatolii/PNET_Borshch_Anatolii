namespace Domain.Model;

public class UserAnswer
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid AttemptId { get; set; }
    public Guid? SelectedAnswerId { get; set; }
    public string? TextAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
 
    public Question Question { get; set; } = null!;
    public TestAttempt Attempt { get; set; } = null!;
    public Answer? SelectedAnswer { get; set; }
}
