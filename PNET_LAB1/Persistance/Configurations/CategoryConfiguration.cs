using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistance.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
 
        builder.HasKey(c => c.Id);
 
        builder.Property(c => c.Id)
            .HasDefaultValueSql("NEWID()");
 
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
 
        builder.HasIndex(c => c.Name)
            .IsUnique();
 
        builder.HasMany(c => c.Tests)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
