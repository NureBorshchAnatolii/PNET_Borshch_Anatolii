using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

 
public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
 
        builder.HasKey(q => q.Id);
 
        builder.Property(q => q.Id)
            .HasDefaultValueSql("NEWID()");
 
        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(2000);
 
        builder.Property(q => q.QuestionType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
 
        builder.Property(q => q.Points)
            .IsRequired()
            .HasColumnType("decimal(5,2)");
 
        builder.Property(q => q.Order)
            .IsRequired();
 
        builder.Property(q => q.Explanation)
            .IsRequired(false)
            .HasMaxLength(2000);
 
        builder.HasOne(q => q.Test)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.HasMany(q => q.UserAnswers)
            .WithOne(ua => ua.Question)
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}