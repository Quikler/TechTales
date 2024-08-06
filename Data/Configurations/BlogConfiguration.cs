using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTales.Data.Models;

namespace TechTales.Data.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<BlogEntity>
{
    public void Configure(EntityTypeBuilder<BlogEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder
            .Property(b => b.Title)
            .HasColumnType("varchar(70)")
            .IsRequired();

        builder
            .Property(b => b.Content)
            .HasColumnType("longtext")
            .IsRequired();

        builder
            .HasMany(b => b.BlogTags)
            .WithOne(bt => bt.Blog)
            .HasForeignKey(bt => bt.BlogId);
    }
}