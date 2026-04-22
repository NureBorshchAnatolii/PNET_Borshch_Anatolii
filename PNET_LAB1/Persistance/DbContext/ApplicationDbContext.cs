using Domain.Enums;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistance.Configurations;

namespace Persistance.DbContext;

public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<TestAttempt> TestAttempts { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new TestConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionConfiguration());
        modelBuilder.ApplyConfiguration(new AnswerConfiguration());
        modelBuilder.ApplyConfiguration(new TestAttemptConfiguration());
        modelBuilder.ApplyConfiguration(new UserAnswerConfiguration());

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Admin",
            LastName = "Admin",
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123Ab"),
            CreateDate = DateTime.UtcNow,
            BirthDate = null,
            Role = UserRole.Admin
        });
    }
}