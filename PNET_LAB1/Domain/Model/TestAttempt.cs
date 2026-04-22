using Domain.Enums;

namespace Domain.Model;

public class TestAttempt
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TestId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
    public decimal? Score { get; set; }
    public int AttemptNumber { get; set; }

    public User User { get; set; } = null!;
    public Test Test { get; set; } = null!;
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}