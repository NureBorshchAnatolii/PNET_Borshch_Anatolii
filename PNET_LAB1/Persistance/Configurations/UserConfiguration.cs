using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.CreateDate)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.BirthDate)
            .IsRequired(false);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.HasMany(u => u.Tests)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.TestAttempts)
            .WithOne(ta => ta.User)
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}