using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechTales.Data.Models;

public class CommentConfiguration : IEntityTypeConfiguration<CommentEntity>
{
    public void Configure(EntityTypeBuilder<CommentEntity> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder
            .Property(c => c.Content)
            .HasColumnType("longtext");

        builder
            .HasOne(c => c.Blog)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BlogId);

        builder
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId);
    }
}