using Domain.Enums;

namespace Domain.Model;

public class Question
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public decimal Points { get; set; }
    public int Order { get; set; }
    public string? Explanation { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}