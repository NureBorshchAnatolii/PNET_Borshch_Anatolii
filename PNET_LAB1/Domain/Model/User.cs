using Domain.Enums;

namespace Domain.Model;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<Test> Tests { get; set; } = new List<Test>();
    public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}