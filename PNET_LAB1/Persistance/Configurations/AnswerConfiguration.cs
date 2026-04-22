using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.IsCorrect)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.Order)
            .IsRequired();

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.UserAnswers)
            .WithOne(ua => ua.SelectedAnswer)
            .HasForeignKey(ua => ua.SelectedAnswerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}