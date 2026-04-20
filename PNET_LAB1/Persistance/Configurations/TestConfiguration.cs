using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
        
        builder.ToTable("Tests");
 
        builder.HasKey(t => t.Id);
 
        builder.Property(t => t.Id)
            .HasDefaultValueSql("NEWID()");
 
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);
 
        builder.Property(t => t.Description)
            .IsRequired(false)
            .HasMaxLength(2000);
 
        builder.Property(t => t.TimeLimitMinutes)
            .IsRequired(false);
 
        builder.Property(t => t.MaxAttempts)
            .IsRequired(false);
 
        builder.Property(t => t.ShuffleQuestions)
            .IsRequired()
            .HasDefaultValue(false);
 
        builder.Property(t => t.ShowCorrectAnswers)
            .IsRequired()
            .HasDefaultValue(false);
 
        builder.Property(t => t.PassingScore)
            .IsRequired()
            .HasColumnType("decimal(5,2)");
 
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");
 
        builder.HasOne(t => t.User)
            .WithMany(u => u.Tests)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
 
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tests)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
 
        builder.HasMany(t => t.Questions)
            .WithOne(q => q.Test)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(t => t.TestAttempts)
            .WithOne(ta => ta.Test)
            .HasForeignKey(ta => ta.TestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}