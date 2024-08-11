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
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .Property(b => b.Content)
            .HasColumnType("longtext")
            .IsRequired();

        builder
            .Property(b => b.Visibility)
            .IsRequired();

        builder
            .HasOne(b => b.Author)
            .WithMany(u => u.Blogs)
            .HasForeignKey(b => b.AuthorId);

        builder
            .HasMany(b => b.Comments)
            .WithOne(c => c.Blog)
            .HasForeignKey(c => c.BlogId);

        builder
            .HasMany(b => b.Tags)
            .WithMany(t => t.Blogs);

        builder
            .HasMany(b => b.Catogories)
            .WithMany(c => c.Blogs);
    }
}