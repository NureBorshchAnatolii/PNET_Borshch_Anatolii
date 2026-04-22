namespace Domain.Model;

public class Test
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShowCorrectAnswers { get; set; }
    public decimal PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}