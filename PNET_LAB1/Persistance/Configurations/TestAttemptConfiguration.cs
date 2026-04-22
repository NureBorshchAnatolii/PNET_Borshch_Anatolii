using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class TestAttemptConfiguration : IEntityTypeConfiguration<TestAttempt>
{
    public void Configure(EntityTypeBuilder<TestAttempt> builder)
    {
        builder.ToTable("TestAttempts");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(ta => ta.StartedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(ta => ta.FinishedAt)
            .IsRequired(false);

        builder.Property(ta => ta.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(ta => ta.Score)
            .IsRequired(false)
            .HasColumnType("decimal(5,2)");

        builder.Property(ta => ta.AttemptNumber)
            .IsRequired();

        builder.HasOne(ta => ta.User)
            .WithMany(u => u.TestAttempts)
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Test)
            .WithMany(t => t.TestAttempts)
            .HasForeignKey(ta => ta.TestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ta => ta.UserAnswers)
            .WithOne(ua => ua.Attempt)
            .HasForeignKey(ua => ua.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}