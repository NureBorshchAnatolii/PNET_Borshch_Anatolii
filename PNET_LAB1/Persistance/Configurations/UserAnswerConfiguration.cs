using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));

        builder.ToTable("UserAnswers");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(ua => ua.SelectedAnswerId)
            .IsRequired(false);

        builder.Property(ua => ua.TextAnswer)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(ua => ua.IsCorrect)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ua => ua.AnsweredAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(ua => ua.Question)
            .WithMany(q => q.UserAnswers)
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ua => ua.Attempt)
            .WithMany(ta => ta.UserAnswers)
            .HasForeignKey(ua => ua.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.SelectedAnswer)
            .WithMany(a => a.UserAnswers)
            .HasForeignKey(ua => ua.SelectedAnswerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}