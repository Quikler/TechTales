using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechTales.Data.Models;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .Property(t => t.Name)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .HasMany(t => t.Blogs)
            .WithMany(b => b.Catogories);
    }
}